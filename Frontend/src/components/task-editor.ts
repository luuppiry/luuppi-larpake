import { isEmpty, LANG_EN, LANG_FI, throwIfAnyNull } from "../helpers";
import { LarpakeTask } from "../models/larpake";

const ID_START = "task-editor-";

export default class TaskEditor extends HTMLElement {
    titleFiField: HTMLInputElement | null = null;
    titleEnField: HTMLInputElement | null = null;
    bodyFiField: HTMLTextAreaElement | null = null;
    bodyEnField: HTMLTextAreaElement | null = null;
    pointsField: HTMLInputElement | null = null;
    headerTitleField: HTMLHeadingElement | null = null;
    headerPointsField: HTMLParagraphElement | null = null;
    idNumber: number | null = null;
    serverTaskId: number | null = null;
    orderingWeightNumber: number = -1;

    constructor() {
        super();
    }

    connectedCallback() {
        this.idNumber = this.getAvailableIdNum();
        const idNum = this.idNumber;

        const header = `
            <div class="_header task-header">
                <div class="task-header-titles">
                    <h4 >
                        <span class="_index"></span>
                        <span class="_header-title">Title</span>
                    </h4>
                    <p>&HorizontalLine;</p>
                    <p class="_header-points">Xp</p>
                    <img class="_delete" src="/icons/bin.svg" alt="delete icon" />
                    <p class="_cancelled hidden">&HorizontalLine; (PERUUTETTU)</p>
                </div>
                <img class="_opened" src="/icons/menu_closed.svg" alt="menu closed icon" />
            </div>
            `;

        const commonInfo = `
            <div class="section-divider">
                <h4>Tehtävä</h4>
                <div class="field">
                    <div class="tooltipped-title" style="margin: 0">
                        <label for="task-${idNum}-points">Pisteitä</label>
                        <img src="/icons/tooltip.svg" alt="tooltip icon" />
                        <p class="tooltip">
                            Tehtävän suorittamisesta annettava pistemäärä
                        </p>
                    </div>
                    <input
                        id="task-${idNum}-points"
                        name="points"
                        class="_points text-field start-year-field"
                        type="number"
                        min="1"
                        max="100"
                        step="1"
                        value="3"
                        required
                    />
                </div>
            </div>
            `;

        const localizedInfo = `
            <div class="section-divider">
                <div>
                    <h4 class="no-margin">Suomeksi (oletus)</h4>
                    <div class="field">
                        <label for="task-${idNum}-title-fi">Otsikko</label>
                        <input
                            id="task-${idNum}-title-fi"
                            name="title-fi"
                            class="_title-fi text-field"
                            placeholder="otsikko suomeksi"
                            maxlength="80"
                            minlength="5"
                            required
                        />
                    </div>
                    <div class="field">
                        <label for="task-${idNum}-body-fi">Selitys</label>
                        <textarea
                            id="task-${idNum}-body-fi"
                            name="body-fi"
                            class="_body-fi description-field text-field"
                            placeholder="kuvaus suomeksi"
                            maxlength="800"
                        ></textarea>
                    </div>
                </div>

                <div>
                    <h4 class="no-margin">Englanniksi</h4>
                    <div class="field">
                        <label for="task-${idNum}-title-en">Otsikko</label>
                        <input
                            id="task-${idNum}-title-en"
                            name="title-en"
                            class="_title-en text-field"
                            placeholder="title in english"
                            maxlength="80"
                            minlength="5"
                            required
                        />
                    </div>
                    <div class="field">
                        <label for="task-${idNum}-body-en">Selitys</label>
                        <textarea
                            id="task-${idNum}-body-en"
                            name="body-en"
                            class="_body-en description-field text-field"
                            placeholder="body in english"
                            maxlength="800"
                        ></textarea>
                    </div>
                </div>
            </div>
            `;

        this.innerHTML = `
        <li class="task">
            ${header}
            <div class="_content task-content">
                ${commonInfo}
                ${localizedInfo}
            </div>
        </li>
        `;

        this.id = `${ID_START}${idNum}`;
        this.titleEnField = this.querySelector<HTMLInputElement>("._title-en");
        this.titleFiField = this.querySelector<HTMLInputElement>("._title-fi");
        this.bodyFiField = this.querySelector<HTMLTextAreaElement>("._body-fi");
        this.bodyEnField = this.querySelector<HTMLTextAreaElement>("._body-en");
        this.pointsField = this.querySelector<HTMLInputElement>("._points");
        this.headerTitleField = this.querySelector<HTMLHeadingElement>("._header-title");
        this.headerPointsField = this.querySelector<HTMLParagraphElement>("._header-points");
        this.querySelector<HTMLSpanElement>("._index")!.innerText = idNum.toString();

        throwIfAnyNull([
            this.titleFiField,
            this.titleEnField,
            this.bodyFiField,
            this.bodyEnField,
            this.pointsField,
            this.headerTitleField,
            this.headerPointsField,
        ]);
    }

    disconnectedCallback() {}

    getAvailableIdNum() {
        let i = 0;
        while (document.getElementById(`${ID_START}${i}`) != null) {
            if (i > 1000) {
                console.log(`Low on available task editor ids, currently searching ${i}`);
            }
            i++;
        }
        return i;
    }

    setData(data: LarpakeTask) {
        this.serverTaskId = data.id;
        this.orderingWeightNumber = data.orderingWeightNumber;

        const textFi = data.textData.filter((x) => x.languageCode == LANG_FI)[0];
        const textEn = data.textData.filter((x) => x.languageCode == LANG_EN)[0];

        if (!textEn?.title) {
            console.log("on");
        }

        this.titleFiField!.value = textFi?.title ?? "";
        this.titleEnField!.value = textEn?.title ?? "";
        this.bodyFiField!.value = textFi?.body ?? "";
        this.bodyEnField!.value = textEn?.body ?? "";
        this.pointsField!.value = data.points.toString();
        this.headerTitleField!.innerText = textFi?.title ?? "Uusi tehtävä";
        this.headerPointsField!.innerText = `${data.points}p`;
        
        if (data.cancelledAt){
            this.querySelector("._cancelled")?.classList.remove("hidden");
        }
        
    }

    addEventListeners() {
        this.querySelector<HTMLElement>("._header")?.addEventListener("click", (_) => {
            // Change content visibility
            const content = this.querySelector<HTMLElement>("._content")!;
            const isOpen = content.style.display === "block";
            content.style.display = isOpen ? "none" : "block";

            // Flip header opened icon
            this.querySelector("._opened")?.classList.toggle("rotate-180deg");
        });

        this.querySelector<HTMLElement>("._delete")?.addEventListener("click", (event) => {
            this.parentElement?.removeChild(this);
            event.stopPropagation(); // To no route click to parent handlers
        });
    }

    addAllEventListeners() {
        addTaskEventListeners();
    }

    getData(): LarpakeTask {
        if (isEmpty(this.titleFiField?.value)) {
            throw new Error(`Task ${this.idNumber} title (fi) cannot be null`);
        }
        if (isEmpty(this.titleEnField?.value)) {
            throw new Error(`Task ${this.idNumber} title (en) cannot be null`);
        }
        if (isEmpty(this.pointsField?.value)) {
            throw new Error(`Task ${this.idNumber} points cannot be null`);
        }
        const points = parseInt(this.pointsField!.value);
        if (Number.isNaN(points)) {
            throw new Error(`Task ${this.idNumber} points must have numeric value`);
        }

        if (isEmpty(this.titleEnField?.value)) {
            console.log("");
        }

        if (this.idNumber == 33){
            console.log("");

        }

        return {
            id: this.serverTaskId ?? -1,
            larpakeSectionId: -1,
            points: points,
            orderingWeightNumber: -1,
            cancelledAt: null,
            createdAt: new Date(),
            updatedAt: new Date(),
            textData: [
                {
                    title: this.titleFiField!.value,
                    body: this.bodyFiField?.value ?? "",
                    languageCode: LANG_FI,
                },
                {
                    title: this.titleEnField!.value,
                    body: this.bodyEnField?.value ?? "",
                    languageCode: LANG_EN,
                },
            ],
        };
    }
}

if ("customElements" in window) {
    customElements.define("task-editor", TaskEditor);
}

export function addTaskEventListeners() {
    let editors = document.getElementsByClassName("._header");
    for (let i = 0; i < editors.length; i++) {
        if (editors[i] instanceof TaskEditor) {
            (editors[i] as TaskEditor).addEventListeners();
        }
    }
}
