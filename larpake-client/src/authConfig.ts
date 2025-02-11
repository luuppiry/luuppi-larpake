import { LogLevel } from "@azure/msal-browser";

export const msalConfig = {
    auth: {
        clientId: "<ClientId>",
        authority:
            "https://<TenantId>.ciamlogin.com/<TenantId>",
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
