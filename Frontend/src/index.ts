import LarpakeClient from "./api_client/larpake_client.ts";

const client = new LarpakeClient();

async function render() {
    const larpakkeet = await client.getOwn();
}

render();
