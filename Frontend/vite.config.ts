import { defineConfig } from "vite";
import path, { resolve } from "path";
import fs from "fs";

const BASE_FOLDER = "public";

const generateFilePaths = (dir: string, skipFiles: boolean = false) => {
    const entries: { [key: string]: string } = {};

    fs.readdirSync(dir).forEach((file) => {
        const fullPath = path.join(dir, file);
        if (fs.statSync(fullPath).isDirectory()) {
            // Found directory, complete recursive search
            Object.assign(entries, generateFilePaths(fullPath, false));
        } else if (!skipFiles && file.endsWith(".html")) {
            // Found, create name from reltive path and without "public" folder
            // Append to entries
            const name = path.relative(path.join(__dirname, BASE_FOLDER), fullPath).replace(".html", "");
            entries[name] = resolve(__dirname, fullPath);
        }
    });
    return entries;
};

export default defineConfig({
    server: {
        port: 3000,
        proxy: {
            "^/(?!.*(fi/|en/)).+\\.html": {
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
    build: {
        outDir: path.resolve(__dirname, "build"),
        emptyOutDir: true,
        rollupOptions: {
            input: generateFilePaths(path.resolve(__dirname, BASE_FOLDER), true)
        },
    },
});
