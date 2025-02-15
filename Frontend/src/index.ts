import EntraId from "./auth.ts";

document.getElementById("auth-btn")?.addEventListener("click", getToken);

async function getToken() {
    console.log("getting token")
    const entra = new EntraId();
    const token = await entra.fetchAzureLogin();
    console.log(token);
}
