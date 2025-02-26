import DevHttpClient from "./dev_http_client.ts";
import EntraId from "./entra_id.ts";

type AccessToken = {
    accessToken: string;
    accessTokenExpiresAt: Date;
    refreshTokenExpiresAt: Date;
};

export default class HttpClient {
    baseUrl: string;
    accessToken: AccessToken | null;

    constructor() {
        this.baseUrl = import.meta.env.VITE_API_BASE_URL;
        this.accessToken = null;
    }

    async get(endpoint: string, query: URLSearchParams | null = null) {
        return await this.makeRequest(endpoint, "GET", null, null, query);
    }

    async post(
        endpoint: string,
        body: any | null = null,
        headers: Headers | null = null
    ) {
        return await this.makeRequest(endpoint, "POST", body, headers, null);
    }

    async put(
        endpoint: string,
        body: any | null = null,
        headers: Headers | null = null
    ) {
        return await this.makeRequest(endpoint, "PUT", body, headers, null);
    }

    async delete(
        endpoint: string,
        body: any | null = null,
        headers: Headers | null = null
    ) {
        return await this.makeRequest(endpoint, "DELETE", body, headers, null);
    }

    /**
     * Makes an HTTP request to the specified endpoint with the given method, headers, and query parameters.
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
        if (import.meta.env.VITE_IS_DEV === "true") {
            return await this.#fetchDev(endpoint, method, body, headers, query);
        }

        return await this.#makeRequestMiddleware(
            endpoint,
            method,
            body,
            headers,
            query
        );
    }

    async #fetchDev(
        endpoint: string,
        method: string = "GET",
        body: any | null = null,
        headers: Headers | null = null,
        query: URLSearchParams | null = null
    ): Promise<Response> {
        const devClient = new DevHttpClient();
        if (
            this.accessToken == null ||
            this.accessToken.accessTokenExpiresAt < new Date()
        ) {
            // Reauthenticate if needed
            this.accessToken = await devClient.authenticate();
        }
        return await devClient.makeDevRequest(
            this.accessToken,
            endpoint,
            method,
            body,
            headers,
            query
        );
    }

    async #makeRequestMiddleware(
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
        headers.append(
            "Authorization",
            `Bearer ${this.accessToken?.accessToken}`
        );

        const url =
            query == null
                ? `${this.baseUrl}${endpoint}`
                : `${this.baseUrl}${endpoint}?${query.toString()}`;

        console.log("fetch first");

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
            console.log(
                `Request to '${endpoint}' failed with status ${response.status}.`
            );
            return response;
        }

        console.log("First request failed, reauthenticating.");
        if ((await this.#renewAccessToken(false)) == false) {
            return response;
        }

        headers.append(
            "Authorization",
            `Bearer ${this.accessToken?.accessToken}`
        );

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
            this.accessToken !== null &&
            this.accessToken.accessTokenExpiresAt > now
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
        return this.accessToken != null;
    }

    async #fetchEntraLogin(): Promise<AccessToken | null> {
        const entra = new EntraId();
        const entraToken = await entra.fetchAzureLogin();
        if (entraToken == null) {
            console.log("Login failed, entra failed.");
            return null;
        }

        const headers = new Headers();
        headers.append("Authorization", `${entraToken}`);

        const response = await fetch(
            `${this.baseUrl}api/authentication/login`,
            {
                method: "POST",
                headers: headers,
                credentials: "include",
            }
        );

        console.log(response);
        if (!response.ok) {
            console.log(
                "Failed to login to API with new entra id access token."
            );
            return null;
        }
        const token = await response.json();
        return token;
    }

    async #fetchRefresh(): Promise<AccessToken | null> {
        const response = await fetch(
            `${this.baseUrl}api/authentication/token/refresh`,
            {
                method: "GET",
                credentials: "include",
            }
        );
        if (!response.ok) {
            console.log("No refresh token exists.");
            return null;
        }
        const token = await response.json();
        return token;
    }
}
