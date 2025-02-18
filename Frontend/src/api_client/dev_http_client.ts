type AccessToken = {
    accessToken: string;
    accessTokenExpiresAt: Date;
    refreshTokenExpiresAt: Date;
};

export default class DevAuth {
    dev_id_guid: string;
    baseUrl: string;

    constructor() {
        if (import.meta.env.VITE_IS_DEV !== "true") {
            throw new Error("Use disabled when VITE_IS_DEV !== 'true'");
        }
        if (import.meta.env.VITE_DEV_ID == undefined) {
            throw new Error("VITE_DEV_ID is not defined fro development");
        }
        this.dev_id_guid = import.meta.env.VITE_DEV_ID;
        this.baseUrl = import.meta.env.VITE_API_BASE_URL;
    }

    async makeDevRequest(
        accessToken: AccessToken,
        endpoint: string,
        method: string = "GET",
        body: any | null = null,
        headers: Headers | null = null,
        query: URLSearchParams | null = null
    ): Promise<Response> {
        headers ??= new Headers();
        headers.append("Content-Type", "application/json");
        headers.append("Authorization", `Bearer ${accessToken.accessToken}`);

        const url =
            query == null
                ? `${this.baseUrl}${endpoint}`
                : `${this.baseUrl}${endpoint}?${query.toString()}`;

        const request: RequestInit = {
            method: method,
            headers: headers,
            body: method === "GET" ? null : JSON.stringify(body),
        };

        return await fetch(url, request);
    }

    async authenticate(): Promise<AccessToken> {
        let response = await fetch(
            `${this.baseUrl}api/authentication/token/refresh`,
            {
                method: "GET",
                credentials: "include",
            }
        );
        if (response.ok) {
            return response.json();
        }

        console.log("(dev) No existing refresh tokens, reauthenticating.");

        const headers = new Headers();
        headers.append("Content-Type", "application/json");

        response = await fetch(
            `${this.baseUrl}api/authentication/login/dummy`,
            {
                method: "POST",
                credentials: "include",
                headers: headers,
                body: JSON.stringify({
                    userId: this.dev_id_guid,
                }),
            }
        );
        if (!response.ok) {
            throw new Error("Dev authentication failed!");
        }
        return response.json();
    }
}
