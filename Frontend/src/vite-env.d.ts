interface ImportMetaEnv {
    readonly VITE_ENTRA_CLIENT_ID: string;
    readonly VITE_ENTRA_SERVER: string;
    readonly VITE_ENTRA_TEDANT_ID: string;
}

interface ImportMeta {
    readonly env: ImportMetaEnv;
}