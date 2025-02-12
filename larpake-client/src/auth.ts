import {
    PublicClientApplication,
    EventType,
    AuthenticationResult,
} from "@azure/msal-browser";
import { msalConfig } from "./authConfig.ts";

export default function createInstance(): PublicClientApplication {
    // This should be initialized outside component tree to prevent re-renders
    const msalInstance = new PublicClientApplication(msalConfig);

    // Activate first account if no active account
    if (
        !msalInstance.getActiveAccount() &&
        msalInstance.getAllAccounts().length > 0
    ) {
        msalInstance.setActiveAccount(msalInstance.getActiveAccount()![0]);
    }

    // Listen to signup event
    msalInstance.addEventCallback((event) => {
        if (
            event.eventType === EventType.LOGIN_SUCCESS &&
            (event.payload as AuthenticationResult).account
        ) {
            const account = (event.payload as AuthenticationResult).account;
            msalInstance.setActiveAccount(account);
        }
    });

    return msalInstance;
}
