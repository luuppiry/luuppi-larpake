import { defineConfig } from "vite";

export default defineConfig({
    server: {
        port: 3000,
        proxy: {
            "^/(?!.*(fi/|en/)).+\\.html$": {
                target: "http://localhost:3000",
                changeOrigin: true,
                rewrite: (path) => {
                    const result = `/fi${path}`;
                    console.log(`dev proxy redirect ${path} -> ${result}`);
                    return result;
                },
            },
        },
    },
});
