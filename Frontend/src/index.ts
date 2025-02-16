import LarpakeClient from "./api_client/larpake_client.ts";

document.getElementById("auth-btn")?.addEventListener("click", getToken);

async function getToken() {
    
    const client = new LarpakeClient();

    const larpakkeet =  await client.getOwn();
    console.log(larpakkeet)
}
