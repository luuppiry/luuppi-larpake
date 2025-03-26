import { formatDate, formatTime, LANG_ATTRIBUTE_NAME, LANG_EN, LANG_FI } from "../helpers.js";
import { EventLocalization, OrgEvent } from "../models/event.js";

export default class EventPreviewer extends HTMLLIElement {
    constructor() {
        super();
    }

    connectedCallback() {
        this.innerHTML = `
                <div class="stripe"></div>
                <div class="section-filler">
                    <a class="_href event-link" >
                        <div class="event">
                            <h3>
                                <span class="_date">1.1.1970</span>
                                <span class="_title">Preview Title</span>
                            </h3>
                            <p class="_time">klo 17:00 &HorizontalLine; 21:00</p>
                            <p class="_body task-body-text">
                                This is event body, a short text about what event 
                                is about...
                            </p>
                        </div>
                    </a>
                </div>
                `;

        this.classList.add("event-item");
        this.classList.add("section");
        this.classList.add("hover-scale");
        this.classList.add("cursor-pointer");
    }

    disconnectedCallback() {}

    setData(event: OrgEvent) {
        const lang = this.getAttribute(LANG_ATTRIBUTE_NAME) === LANG_EN ? LANG_EN : LANG_FI;

        const text: EventLocalization = event.textData.filter((x) => x.languageCode == lang)[0] ?? event.textData[0];

        this.querySelector<HTMLSpanElement>("._date")!.innerText = formatDate(event.startsAt);
        this.querySelector<HTMLSpanElement>("._title")!.innerText = text.title;
        this.querySelector<HTMLParagraphElement>("._time")!.innerText = formatTime(event.startsAt);
        this.querySelector<HTMLParagraphElement>("._body")!.innerText = text.body;
        this.querySelector<HTMLAnchorElement>("._href")!.href = text.websiteUrl;
    }
}
if ("customElements" in window) {
    customElements.define("event-previewer", EventPreviewer, { extends: "li" });
}
