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

    if (larpakkeet.length === 0) {
        const lang = getDocumentLangCode();
        const translations = {
            fi: {
                emptyTitle: "Tyhjää täynnä!",
                emptyDesc1: "Et ole osallistunut vielä yhteenkään Lärpäkkeeseen.",
                emptyDesc2: "Jos kuitenkin tiedät olevasi merkattu osallistujana Lärpäkkeeseen, saattaa vika olla palvelimella tai nettiyhteydessä. Jos ongelma toistuu, ota yhteyttä ylläpitoon.",
            },
            en: {
                emptyTitle: "Full of emptiness!",
                emptyDesc1: "You have not yet attended any Lärpäke.",
                emptyDesc2: "If you know you to be attending some Lärpäke, the problem might lie somewhere on our servers or internet connection. If this keeps happening, contact system administration.",
            },
        };

        const t = translations[lang]; // Pick the right language
        container.innerHTML = `
            <section class="container">
                <h2>${t.emptyTitle}</h2>
                <p>${t.emptyDesc1}</p>
                <p>${t.emptyDesc2}</p>
            </section>
        `;
        return;
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
