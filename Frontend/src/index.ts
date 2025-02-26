import LarpakeClient from "./api_client/larpake_client.ts";
import EntraId from "./api_client/entra_id.ts";

document.getElementById("auth-btn")?.addEventListener("click", getToken);

async function getToken() {
    
    

    const client = new LarpakeClient();

    const larpakkeet =  await client.getOwn();
    console.log(larpakkeet)
}
