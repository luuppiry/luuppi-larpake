import {
    AccountInfo,
    Configuration,
    InteractionRequiredAuthError,
    IPublicClientApplication,
    PopupRequest,
    PublicClientApplication,
    RedirectRequest,
    SilentRequest,
} from "@azure/msal-browser";

export default class EntraId {
    config: Configuration;
    msalInstance: IPublicClientApplication | null = null;
    accounts: AccountInfo[] | null = null;
    isInitialized: boolean = false;

    constructor() {
        this.config = {
            auth: {
                clientId: import.meta.env.VITE_ENTRA_CLIENT_ID,
                authority: `https://${import.meta.env.VITE_ENTRA_SERVER}.ciamlogin.com/${
                    import.meta.env.VITE_ENTRA_TEDANT_ID
                }`,
                redirectUri: import.meta.env.VITE_ENTRA_REDIRECT_URL,
            },
        };
    }

    async fetchAzureLogin(): Promise<string | null> {
        await this.#initialize();

        const redirectResponse = await this.msalInstance?.handleRedirectPromise();
        if (redirectResponse != null) {
            if (redirectResponse.accessToken) {
                return redirectResponse.accessToken;
            }
        }

        const request = await this.#createRequest();

        if (this.accounts?.length! > 0) {
            const token = await this.fetchSilent(request);
            if (token !== null) {
                return token;
            }
        }
        await this.fetchRedirect(request);
        throw new Error("Redirect failed, failed to authenicate user")
    }

    async fetchSilent(request: SilentRequest): Promise<string | null> {
        await this.#initialize();

        try {
            const token = await this.msalInstance!.acquireTokenSilent(request);
            this.#addDistinctAccount(token.account);
            return token.accessToken;
        } catch (error) {
            if (error instanceof InteractionRequiredAuthError) {
                console.log("No silent token in memory.");
                return null;
            }
            console.log(error);
            return null;
        }
    }

    async fetchPopup(request: PopupRequest): Promise<string | null> {
        await this.#initialize();
        try {
            const token = await this.msalInstance!.acquireTokenPopup(request);
            this.#addDistinctAccount(token.account);
            return token.accessToken;
        } catch (error) {
            console.log(error);
            return null;
        }
    }

    async fetchRedirect(request: RedirectRequest): Promise<void> {
        await this.#initialize();
        try {
            await this.msalInstance!.acquireTokenRedirect(request);
        } catch (error) {
            console.log(error);
        }
    }

    async fetchAzureLogout() {
        await this.#initialize();
        try {
            await this.msalInstance!.logoutPopup();
            this.accounts = [];
            return "Logout succesful";
        } catch (error) {
            console.log("Logout failed:", error);
        }
    }

    async #createRequest() {
        await this.#initialize();

        const scope = import.meta.env.VITE_ENTRA_SCOPE;
        return this.accounts!.length > 0
            ? {
                  scopes: [scope],
                  account: this.accounts![0],
              }
            : { scopes: [scope] };
    }

    async #initialize() {
        if (this.isInitialized) {
            return;
        }
        //  Create client if not already initialized.
        if (this.msalInstance === null) {
            this.isInitialized = true;
            this.msalInstance = await PublicClientApplication.createPublicClientApplication(
                this.config
            );
            this.accounts = this.msalInstance.getAllAccounts() ?? [];
        }
    }

    async #addDistinctAccount(account: AccountInfo) {
        if (!this.accounts?.find((x) => x.homeAccountId === account.homeAccountId)) {
            this.accounts?.push(account);
        }
    }
}
