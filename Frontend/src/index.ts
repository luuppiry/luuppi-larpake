import { EventClient } from "./api_client/event_client.js";
import LarpakeClient from "./api_client/larpake_client.js";
import EventPreviewer from "./components/event-previewer.js";
import { appendTemplateElement, getDocumentLangCode, removeChildren } from "./helpers.js";

const eventClient = new EventClient();
const larpakeClient = new LarpakeClient(eventClient.client);

async function render() {
    await loadLarpakkeet();
    await loadComingEvents();
}

async function loadLarpakkeet() {
    const container = document.getElementById("larpake-container") as HTMLUListElement;
    if (!container) {
        throw new Error("Larpake container not found, check naming.");
    }

    const larpakkeet = await larpakeClient.getOwn();
    if (larpakkeet == null) {
        throw new Error("Could not load larpakkeet from server.");
    }

    removeChildren(container);
    for (const larpake of larpakkeet) {
        const element = appendTemplateElement<HTMLElement>("larpake-template", container);

        const lang = getDocumentLangCode();
        const text =
            larpake.textData.filter((x) => x.languageCode == lang)[0] ?? larpake.textData[0];

        const params = new URLSearchParams();
        params.append("LarpakeId", larpake.id.toString());

        element.querySelector<HTMLHeadingElement>("._title")!.innerText = text.title;
        element.querySelector<HTMLAnchorElement>(
            "._href"
        )!.href = `larpake.html?${params.toString()}`;
        element.querySelector<HTMLImageElement>("._img")!.src = larpake.image_url ?? "/kiasa.png";
    }
}

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

render();
