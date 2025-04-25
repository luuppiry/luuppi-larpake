import { Q_NO_TOKEN } from "./constants.js";

const index = document.querySelectorAll("._home-redirect");
index.forEach((x) => {
    const link = x as HTMLAnchorElement;
    if (link && link.href.includes("index")) {
        const url = new URL(link.href);
        const params = new URLSearchParams(url.searchParams);
        params.set(Q_NO_TOKEN, "false");
        window.location.href = `${url.host}?${params}`;
    }
});
