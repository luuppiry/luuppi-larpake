import { EntraId } from "./auth-client.ts";
import { IMsalContext, useMsal } from "@azure/msal-react";

type RefreshToken = {
    message: string;
    accessToken: string;
    accessTokenExpiresAt: Date;
    refreshTokenExpiresAt: Date;
    tokenType: string;
};

type Request = {
    method: string;
    headers: Headers;
    body: string;
};

export default function useAuthenticatedClient() {
    const ctx = useMsal();
    return new HttpClient(ctx);
}

export class HttpClient {
    #entraId: EntraId;
    #credentials: RefreshToken | null;

    constructor(msal: IMsalContext) {
        this.#entraId = new EntraId(msal);
        this.#credentials = null;
    }

    async fetch(
        endpoint: string,
        method: string = "GET",
        body: any = null,
        query: URLSearchParams | null = null
    ): Promise<any> {
        const host = process.env.REACT_APP_API_HOST;
        let url = `${host}/api/${endpoint}}`;

        if (query instanceof URLSearchParams) {
            url = `${url}?${query.toString()}`;
        }

        let headers = new Headers();
        headers.append("Content-Type", "application/json");

        let request: Request = {
            method: method,
            headers: headers,
            body: JSON.stringify(body),
        };

        const response = await this.#runWithMiddleware(url, request);
        if (response.ok) {
            return response;
        }

        // If unauthorized (token might be invalid)
        if (response.status === 401) {
            // Reset credentials and retry
            this.#credentials = null;
            return await this.#runWithMiddleware(url, request);
        }
        return response;
    }

    async #runWithMiddleware(url: string, request: Request) {
        // Try refresh new credentials
        if (
            this.#credentials === null ||
            this.#credentials.accessTokenExpiresAt < new Date()
        ) {
            this.#credentials = await this.#apiRefreshToken();
        }

        // Get id token from Azure if needed
        if (this.#credentials === null) {
            const idToken = await this.#entraId.fetchAzurelogin();
            if (idToken === null) {
                throw new Error("Auth failed");
            }
            this.#credentials = await this.#apiLogin(idToken);
        }

        /* At this point we should have valid credentials
         * - Tokens are refreshed (if needed)
         * - New id token is aquired (if needed)
         *
         */

        // Add access token
        request.headers.append(
            "Authorization",
            `Bearer ${this.#credentials.accessToken}`
        );

        // Make the *real* api call
        return await fetch(url, request);
    }

    async #apiRefreshToken(): Promise<RefreshToken | null> {
        const url = `${process.env.REACT_APP_API_HOST}api/authentication/token/refresh`;

        const response = await fetch(url, {
            method: "GET",
        });

        if (!response.ok) {
            console.log(`Failed to refresh: ${response.statusText}`);
            return null;
        }
        const token = await response.json();
        return token;
    }

    async #apiLogin(idToken: string): Promise<RefreshToken> {
        const url = `${process.env.REACT_APP_API_HOST}api/authentication/login`;
        const headers = new Headers();
        headers.append("Authorization", `Bearer ${idToken}`);

        const response = await fetch(url, {
            method: "POST",
            headers: headers,
        });

        if (!response.ok) {
            throw new Error(
                "Failed to login to api, maybe token is in incorrect format."
            );
        }

        const token: RefreshToken = await response.json();
        return token;
    }

    async #fetchDummyLogin(userId: string) {
        const response = await fetch(
            `${process.env.REACT_APP_API_HOST}api/authentication/login/dummy`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    userId: userId,
                }),
            }
        );

        if (!response.ok) {
            throw new Error("Network error");
        }
        return await response.json();
    }
}
