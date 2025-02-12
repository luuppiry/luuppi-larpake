import {
    AccountInfo,
    InteractionRequiredAuthError,
    IPublicClientApplication,
    PopupRequest,
    SilentRequest,
} from "@azure/msal-browser";
import { IMsalContext } from "@azure/msal-react";

export class EntraId {
    msalInstance: IPublicClientApplication;
    accounts: AccountInfo[];

    constructor(
        msal: IMsalContext,
    ) {
        this.msalInstance = msal.instance;
        this.accounts = msal.accounts ?? [];
    }

    
    async fetchAzurelogin(): Promise<string | null> {
        const request = this.getRequest();

        if (this.accounts.length > 0) {
            const silentToken = await this.aquireEntraSilent(request);
            if (silentToken === undefined) {
                return null;
            }
            if (silentToken !== null) {
                return silentToken;
            }
        }

        const popupToken = await this.aquireEntraPopup(request);
        if (popupToken === null) {
            console.log("Authentication failed.");
        }
        return popupToken;
    }

    async aquireEntraSilent(
        request: SilentRequest
    ): Promise<string | null | undefined> {
        try {
            // Get token from already singed in account
            const token = await this.msalInstance.acquireTokenSilent(request);
            return token.accessToken;
        } catch (error) {
            if (error instanceof InteractionRequiredAuthError) {
                return null;
            }

            console.log(error);
            return undefined;
        }
    }

    async aquireEntraPopup(request: PopupRequest): Promise<string | null> {
        try {
            // Ask user creadentials to retrieve access token
            const token = await this.msalInstance.acquireTokenPopup(request);
            return token.accessToken;
        } catch (error) {
            console.log(error);
            return null;
        }
    }

    getRequest() {
        if (this.accounts.length > 0) {
            return {
                scopes: [],
                account: this.accounts[0],
            };
        }
        return {
            scopes: [],
        };
    }
}
