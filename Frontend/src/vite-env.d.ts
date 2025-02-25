interface ImportMetaEnv {
    readonly VITE_ENTRA_CLIENT_ID: string;
    readonly VITE_ENTRA_SERVER: string;
    readonly VITE_ENTRA_TEDANT_ID: string;
    readonly VITE_ENTRA_SCOPE: string;
    readonly VITE_API_BASE_URL: string;
    readonly VITE_ENTRA_REDIRECT_URL: string;
    readonly VITE_IS_DEV: string;
    readonly VITE_DEV_ID: string;
}

interface ImportMeta {
    readonly env: ImportMetaEnv;
}
