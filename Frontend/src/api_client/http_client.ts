import { UserAuthenticatedEvent, UserInfo } from "../models/common.js";
import EntraId from "./entra_id.js";

export const AUTHENTICATED_EVENT_NAME = "client-authenticated";

type AccessToken = {
    accessToken: string;
    accessTokenExpiresAt: Date;
    refreshTokenExpiresAt: Date;
    permissions: number;
};

export default class HttpClient {
    baseUrl: string;
    accessToken: AccessToken | null;

    constructor() {
        this.baseUrl = import.meta.env.VITE_API_BASE_URL;
        if (!this.baseUrl) {
            throw new Error("Api base url is not provided, check server configuration.");
        }
        this.accessToken = null;
    }

    async get(endpoint: string, query: URLSearchParams | null = null) {
        return await this.makeRequest(endpoint, "GET", null, null, query);
    }

    async post(endpoint: string, body: any | null = null, headers: Headers | null = null) {
        return await this.makeRequest(endpoint, "POST", body, headers, null);
    }

    async put(endpoint: string, body: any | null = null, headers: Headers | null = null) {
        return await this.makeRequest(endpoint, "PUT", body, headers, null);
    }

    async delete(endpoint: string, body: any | null = null, headers: Headers | null = null) {
        return await this.makeRequest(endpoint, "DELETE", body, headers, null);
    }

    async logout(): Promise<boolean> {
        this.accessToken = null;

        const response = await this.post("api/authentication/token/invalidate");
        if (!response.ok) {
            console.warn(
                "Failed to invalidate refresh token on Lärpäke API:",
                await response.json()
            );
        }

        const entra = new EntraId();
        const entraSuccess = await entra.fetchAzureLogout();
        if (!entraSuccess) {
            console.warn("Failed to logout from entra");
        }
        return entraSuccess !== undefined;
    }

    async login(): Promise<UserInfo | null> {
        if (this.accessToken?.permissions) {
            return {
                permissions: this.accessToken.permissions,
            };
        }
        await this.#renewAccessToken();
        if (this.accessToken?.permissions) {
            return {
                permissions: this.accessToken!.permissions,
            };
        }
        return null;
    }

    async trySilentLogin(): Promise<UserInfo | null> {
        // Already logged in
        if (this.accessToken?.permissions) {
            return {
                permissions: this.accessToken.permissions,
            };
        }

        // Try API token refresh
        const resp = await this.#fetchRefresh();
        if (resp) {
            return {
                permissions: resp.permissions,
            };
        }

        // Fetch silent
        const entra = new EntraId();
        const request = await entra.createRequest();

        const azureToken = await entra.fetchSilent(request);
        if (!azureToken) {
            return null;
        }

        const accessToken = await this.#fetchApiLogin(azureToken);
        if (!accessToken) {
            return null;
        }
        this.accessToken = accessToken;
        this.#authenticated(this.accessToken);

        return {
            permissions: accessToken.permissions,
        };
    }

    /* Makes an HTTP request to the specified endpoint with the given method, headers, and query parameters.
     * Ensures valid access tokens if can get one, asks user credentials if needed.
     *
     * @param endpoint - The endpoint to which the request is made, do not include starting "/".
     * @param method - The HTTP method to use for the request (default is "GET").
     * @param headers - The headers to include in the request (default is null).
     * @param query - The query parameters to include in the request (default is null).
     * @returns A promise that resolves to the response of the request.
     * @throws An error if authentication fails.
     */
    async makeRequest(
        endpoint: string,
        method: string = "GET",
        body: any | null = null,
        headers: Headers | null = null,
        query: URLSearchParams | null = null
    ): Promise<Response> {
        if ((await this.#ensureAccessToken()) == false) {
            throw new Error("Authentication failed.");
        }

        // First attempt
        headers ??= new Headers();
        headers.append("Content-Type", "application/json");
        headers.set("Authorization", `Bearer ${this.accessToken?.accessToken}`);

        const url =
            query == null
                ? `${this.baseUrl}${endpoint}`
                : `${this.baseUrl}${endpoint}?${query.toString()}`;

        console.log("Making api request");

        const request: RequestInit = {
            method: method,
            headers: headers,
            body: method === "GET" ? null : JSON.stringify(body),
        };

        const response = await fetch(url, request);

        // If valid request
        if (response.ok) {
            return response;
        }

        // If not unauthorized (some error)
        if (response.status != 401) {
            console.log(`Request to '${endpoint}' failed with status ${response.status}.`);
            return response;
        }

        console.log("First request failed, reauthenticating.");
        if ((await this.#renewAccessToken(false)) == false) {
            return response;
        }

        headers.append("Authorization", `Bearer ${this.accessToken?.accessToken}`);

        console.log("fetch second");
        return await fetch(url, {
            method: method,
            headers: headers,
            body: method === "GET" ? null : JSON.stringify(body),
        });
    }

    async #ensureAccessToken(): Promise<boolean> {
        const now = new Date();
        if (
            this.accessToken != null &&
            this.accessToken.accessToken != null &&
            new Date(this.accessToken.accessTokenExpiresAt) > now
        ) {
            // Valid token exists
            return true;
        }

        console.log("Renew token");
        return await this.#renewAccessToken();
    }

    async #renewAccessToken(tryRefresh: boolean = true): Promise<boolean> {
        console.log("getting token");

        if (tryRefresh) {
            this.accessToken = await this.#fetchRefresh();
            if (this.accessToken != null) {
                return true;
            }
        }

        this.accessToken = await this.#fetchEntraLogin();
        if (this.accessToken) {
            this.#authenticated(this.accessToken);
        }
        return this.accessToken != null;
    }

    async #fetchEntraLogin(): Promise<AccessToken | null> {
        const entra = new EntraId();
        const entraToken = await entra.fetchAzureLogin();
        if (entraToken == null) {
            console.log("Login failed, entra failed.");
            return null;
        }
        return await this.#fetchApiLogin(entraToken);
    }

    async #fetchRefresh(): Promise<AccessToken | null> {
        const response = await fetch(`${this.baseUrl}api/authentication/token/refresh`, {
            method: "GET",
            credentials: "include",
        });
        if (!response.ok) {
            console.log("No refresh token exists.");
            return null;
        }
        const token = await response.json();
        return token;
    }

    async #fetchApiLogin(entraToken: string): Promise<AccessToken | null> {
        const headers = new Headers();
        headers.append("Authorization", `Bearer ${entraToken}`);
        headers.append("Accept", "*/*");

        const response = await fetch(`${this.baseUrl}api/authentication/login`, {
            method: "POST",
            headers: headers,
            credentials: "include",
        });

        if (!response.ok) {
            console.warn("Failed to login to API with new entra id access token.");
            return null;
        }
        const token = await response.json();
        return token;
    }

    async #authenticated(token: AccessToken) {
        const data: UserInfo = {
            permissions: token.permissions
        }

        const event: UserAuthenticatedEvent = new CustomEvent(AUTHENTICATED_EVENT_NAME, {
            detail: data
        })

        document.dispatchEvent(event);

    }
}
