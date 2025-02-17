# How To Make Atuhentication Work

## 1. Client Side

Update .env with correct values

```
VITE_ENTRA_CLIENT_ID = <Azure client id>
VITE_ENTRA_TEDANT_ID = <Azure tedant id>
VITE_ENTRA_SERVER = luuppiweba
VITE_ENTRA_SCOPE = api://luuppi-larpake/Larpake.Use
VITE_API_BASE_URL = https://localhost:7267/
```

Then

-   `Clear browser cookies and caches for this frontend locahost port`

## 2. Server Side

Update appsettings.Development.json (Add inside top level object)

```json
{
    "EntraId": {
        "Instance": "https://luuppiweba.ciamlogin.com",
        "ClientId": "6b19d245-0286-43a2-8481-98db51a8e777", // From Azure
        "TenantId": "e066975d-a520-4d16-bc9e-861a5ed8ded7", // From Azure
        "Scopes": "Larpake.Use"
    }
}
```

## 3. Run

Run server

```ps
dotnet run build --launch-profile https
```

Run client

```ps
npm run dev
```

-   Sign in in client with entra id by pressing login button

## 4. After Successful login

By default user does not have any permissions (like reading data from server). User can be given permissions through admin panel, by adding to fuksi group or by server config. Now we give admin permissions by server config.

-   Stop API server
-   Open appsettings.Development.json
-   Search database users table for your newly generated user id (not the entra id), you entra username/email should be found in the table if authentication was correctly made

Add section inside top level object

```json
{
    "Permissions": {
        "SetOnStartup": true,
        "FailIfNotFound": true,
        "Sudo": [
            "0195138c-7eec-773b-8993-a35d4e8d164c" // Owner user_id here
        ]
    }
}
```

-   Restart server
-   You should now have all permissions
-   This way should be used for ONLY ADMINS
-   Note that if you haven't signed in chrome, browser might reset all cookies (like refresh token) when refreshing the page. This causes hitting more azure entra servers and might cause slower login times and refresh tokens do not work.
