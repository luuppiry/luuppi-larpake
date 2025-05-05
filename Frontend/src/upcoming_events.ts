import EventPreviewer from "./components/event-previewer.js";
import { EventClient } from "./api_client/event_client.js";

const eventClient = new EventClient();

async function loadComingEvents() {
    const container = document.getElementById("event-container") as HTMLUListElement;
    if (!container) {
        throw new Error("Event container not found, check naming.");
    }

    const events = await eventClient.nextComing();
    if (!events) {
        throw new Error("Failed to load coming events");
    }

    for (const event of events.sort((x, y) => (x.startsAt > y.startsAt ? 1 : -1))) {
        const elem = document.createElement("li", { is: "event-previewer" }) as EventPreviewer;
        container.appendChild(elem);
        elem.setData(event);
    }
}

loadComingEvents();
