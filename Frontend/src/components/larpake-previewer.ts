import { Q_LARPAKE_ID } from "../constants.js";
import { throwIfAnyNull } from "../helpers.js";

const idStart = "larpake-preview-";

export type PreviewData = {
    id: number;
    titleFi: string;
    year: number | null;
    sections: number;
    tasks: number;
    points: number;
    groups: number;
    createdAt: Date;
    updatedAt: Date;
    imgUrl: string;
};

export default class LarpakePreview extends HTMLLIElement {
    link: HTMLAnchorElement | null = null;
    image: HTMLImageElement | null = null;
    titleText: HTMLElement | null = null;
    year: HTMLElement | null = null;
    sections: HTMLElement | null = null;
    tasks: HTMLElement | null = null;
    total: HTMLElement | null = null;
    groups: HTMLElement | null = null;
    created: HTMLElement | null = null;
    edited: HTMLElement | null = null;

    constructor() {
        super();
    }

    connectedCallback() {
        const numId = this.#getAvailableIdNum();

        this.innerHTML = `
            <article class="hover-scale">
                <a class="_link link-section" style="margin: 0px">
                    <form-container>
                        <h3 class="_title" class="larpake-title">Kiasan SeikkailuLärpäke 2024</h3>
                        <div style="display: flex" class="cursor-pointer">
                            <div class="data-container" style="flex: 1">
                                <p>Vuosi: <b class="_year">20XX</b></p>
                                <p>Osioita: <b class="_sections">X</b> kpl</p>
                                <p>Tehtäviä: <b class="_tasks">X</b> kpl</p>
                                <p>Yhteensä: <b class="_points">X</b> pistettä</p>
                                <p>Ryhmiä: <b class="_groups">X</b> kpl</p>

                                <p class="line-breaking">
                                    <span class="line">Luotu: </span>
                                    <span class="_created" class="line">20.2.2024 klo 12:30</span>
                                </p>
                                <p class="line-breaking">
                                    <span>Muokattu: </span>
                                    <span class="_edited">24.2.2024 klo 00.23</span>
                                </p>
                            </div>
                            <div class="image-container">
                                <img
                                    class="_image"
                                    src="/kiasa.png"
                                    style="aspect-ratio: 1; width: 100%"
                                />
                            </div>
                        </div>
                    </form-container>
                </a>
            </article>
            
            `;
        this.id = `${idStart}${numId}`;
        this.link = this.querySelector<HTMLAnchorElement>(`._link`)!;
        this.image = this.querySelector<HTMLImageElement>(`._image`)!;
        this.titleText = this.querySelector<HTMLElement>(`._title`)!;
        this.year = this.querySelector<HTMLElement>(`._year`)!;
        this.sections = this.querySelector<HTMLElement>(`._sections`)!;
        this.tasks = this.querySelector<HTMLElement>(`._tasks`)!;
        this.total = this.querySelector<HTMLElement>(`._points`)!;
        this.groups = this.querySelector<HTMLElement>(`._groups`)!;
        this.created = this.querySelector<HTMLElement>(`._created`)!;
        this.edited = this.querySelector<HTMLElement>(`._edited`)!;

        throwIfAnyNull([
            this.link,
            this.image,
            this.titleText,
            this.year,
            this.sections,
            this.tasks,
            this.total,
            this.groups,
            this.created,
            this.edited,
        ]);
    }

    disconnectedCallback() {}

    addData(data: PreviewData) {
        // ctor validates all to be not null
        this.link!.href = `larpake_manager.html?${Q_LARPAKE_ID}=${data.id}`;
        this.image!.src = data.imgUrl;
        this.titleText!.innerText = data.titleFi;
        this.year!.innerText = data.year?.toString() ?? "N/A";
        this.sections!.innerText = data.sections.toString();
        this.tasks!.innerText = data.tasks.toString();
        this.total!.innerText = data.points.toString();
        this.groups!.innerText = data.groups.toString();
        this.created!.innerText = data.createdAt.toString().split("T")[0];
        this.edited!.innerText = data.updatedAt.toString().split("T")[0];
    }

    #getAvailableIdNum() {
        let i = 0;
        while (document.getElementById(`${idStart}${i}`) != null) {
            if (i > 1000) {
                console.log(`Low on available section editor ids, currently searching ${i}`);
            }
            i++;
        }
        return i;
    }
}

if ("customElements" in window) {
    customElements.define("larpake-preview", LarpakePreview, { extends: "li" });
}
