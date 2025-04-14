import { EventClient } from "../api_client/event_client";

const client = new EventClient();

document.getElementById("pull-events-btn")?.addEventListener("click", async _ => {
    try {
        await client.pullExternal();
        alert("Success!\nEvents pulled successfully")
    }
    catch (error){
        alert(`Action failed! \n${error}`)
    }
});