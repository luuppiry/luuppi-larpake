import { LogLevel } from "@azure/msal-browser";

export const msalConfig = {
    auth: {
        clientId: process.env.REACT_APP_ENTRA_CLIENT_ID!,
        authority: `https://${process.env.REACT_APP_ENTRA_TENANT_ID}.ciamlogin.com/${process.env.REACT_APP_ENTRA_TENANT_ID}`,
        redirectUri: "http://localhost:3000",
    },
    cache: {
        cacheLocation: "sessionsStorage",
        storeAuthStateInCookie: false,
    },
    system: {
        loggerOptions: {
            loggerCallback: (level: any, message: any, containsPii: any) => {
                if (containsPii) {
                    return;
                }
                switch (level) {
                    case LogLevel.Error:
                        console.error(message);
                        return;
                    case LogLevel.Warning:
                        console.warn(message);
                        return;
                    case LogLevel.Info:
                        console.info(message);
                        return;
                    case LogLevel.Verbose:
                        console.debug(message);
                        return;
                    default:
                        return;
                }
            },
        },
    },
};

export const loginRequest = {
    scopes: [],
};
