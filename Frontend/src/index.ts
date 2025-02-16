import HttpClient from "./http_client.ts";

document.getElementById("auth-btn")?.addEventListener("click", getToken);

async function getToken() {
    const client = new HttpClient();
    const response = await client.makeRequest("api/larpakkeet/own");

    console.log(response);
}
