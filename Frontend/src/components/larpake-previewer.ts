import { Q_LARPAKE_ID } from "../constants";
import { throwIfAnyNull } from "../helpers";

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
    idNumber: number | null = null;
    titleImage: HTMLImageElement | null = null;
    link: HTMLAnchorElement | null = null;
    image: HTMLImageElement | null = null;
    titleText: HTMLHeadingElement | null = null;
    year: HTMLParagraphElement | null = null;
    sections: HTMLParagraphElement | null = null;
    tasks: HTMLParagraphElement | null = null;
    total: HTMLParagraphElement | null = null;
    groups: HTMLParagraphElement | null = null;
    created: HTMLSpanElement | null = null;
    edited: HTMLSpanElement | null = null;

    constructor() {
        super();
    }

    connectedCallback() {
        const numId = this.#getAvailableIdNum();

        this.innerHTML = `
            <a id="larpake-${numId}-link" class="section hover-scale link-section" style="margin: 0px">
                <div class="stripe"></div>
                <div class="section-filler">
                    <div>
                        <h3 id="larpake-${numId}-title" class="larpake-title">Kiasan SeikkailuLärpäke 2024</h3>
                        <div style="display: flex" class="cursor-pointer">
                            <div class="data-container" style="flex: 1">
                                <p>Vuosi: <b id="larpake-${numId}-year">20XX</b></p>
                                <p>Osioita: <b id="larpake-${numId}-sections">X</b> kpl</p>
                                <p>Tehtäviä: <b id="larpake-${numId}-tasks">X</b> kpl</p>
                                <p>Yhteensä: <b id="larpake-${numId}-total">X</b> pistettä</p>
                                <p>Ryhmiä: <b id="larpake-${numId}-groups">X</b> kpl</p>

                                <p class="line-breaking">
                                    <span class="line">Luotu: </span>
                                    <span id="larpake-${numId}-created" class="line">20.2.2024 klo 12:30</span>
                                </p>
                                <p class="line-breaking">
                                    <span>Muokattu: </span>
                                    <span id="larpake-${numId}-edited">24.2.2024 klo 00.23</span>
                                </p>
                            </div>
                            <div class="image-container">
                                <img id="larpake-${numId}-title-image" src="/kiasa.png" style="aspect-ratio: 1; width: 100%" />
                            </div>
                        </div>
                    </div>
                </div>
            </a>
            `;
        this.id = `${idStart}${numId}`;
        this.link = document.getElementById(`larpake-${numId}-link`) as HTMLAnchorElement;
        this.image = document.getElementById(`larpake-${numId}-title-image`) as HTMLImageElement;
        this.titleText = document.getElementById(`larpake-${numId}-title`) as HTMLHeadingElement;
        this.year = document.getElementById(`larpake-${numId}-year`) as HTMLParagraphElement;
        this.sections = document.getElementById(`larpake-${numId}-sections`) as HTMLParagraphElement;
        this.tasks = document.getElementById(`larpake-${numId}-tasks`) as HTMLParagraphElement;
        this.total = document.getElementById(`larpake-${numId}-total`) as HTMLParagraphElement;
        this.groups = document.getElementById(`larpake-${numId}-groups`) as HTMLParagraphElement;
        this.created = document.getElementById(`larpake-${numId}-created`) as HTMLSpanElement;
        this.edited = document.getElementById(`larpake-${numId}-edited`) as HTMLSpanElement;

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
