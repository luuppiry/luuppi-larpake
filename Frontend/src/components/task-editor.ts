import { isEmpty, LANG_EN, LANG_FI } from "../helpers";
import { LarpakeTask } from "../models/larpake";

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
            <div id="task-${idNum}-header" class="task-header">
                <div class="task-header-titles">
                    <h4 id="task-${idNum}-header-title">Title</h4>
                    <p>&HorizontalLine;</p>
                    <p id="task-${idNum}-header-points">Xp</p>
                    <img id="delete-task-${idNum}-btn" src="/icons/bin.svg" alt="delete icon" />
                </div>
                <img id="task-${idNum}-opened-img" src="/icons/menu_closed.svg" alt="menu closed icon" />
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
                        class="text-field start-year-field"
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
                            name="title-en"
                            class="text-field"
                            placeholder="otsikko suomeksi"
                            maxlength="80"
                            minlenght="3"
                            required
                        />
                    </div>
                    <div class="field">
                        <label for="task-${idNum}-body-fi">Selitys</label>
                        <textarea
                            id="task-${idNum}-body-fi"
                            name="body-fi"
                            class="description-field text-field"
                            placeholder="kuvaus suomeksi"
                            maxlength="800"
                            minlenght="3"
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
                            class="text-field"
                            placeholder="title in english"
                            maxlength="80"
                            minlenght="3"
                            required
                        />
                    </div>
                    <div class="field">
                        <label for="task-${idNum}-body-en">Selitys</label>
                        <textarea
                            id="task-${idNum}-body-en"
                            name="body-en"
                            class="description-field text-field"
                            placeholder="body in english"
                            maxlength="800"
                            minlenght="3"
                        ></textarea>
                    </div>
                </div>
            </div>
            `;

        this.innerHTML = `
        <li class="task">
            ${header}
            <div id="task-${idNum}-content" class="task-content">
                ${commonInfo}
                ${localizedInfo}
            </div>
        </li>
        `;

        this.id = `task-editor-${idNum}`;
        this.titleFiField = document.getElementById(`task-${idNum}-title-fi`) as HTMLInputElement;
        this.titleEnField = document.getElementById(`task-${idNum}-title-en`) as HTMLInputElement;
        this.bodyFiField = document.getElementById(`task-${idNum}-body-fi`) as HTMLTextAreaElement;
        this.bodyEnField = document.getElementById(`task-${idNum}-body-en`) as HTMLTextAreaElement;
        this.pointsField = document.getElementById(`task-${idNum}-points`) as HTMLInputElement;
        this.headerTitleField = document.getElementById(`task-${idNum}-header-title`) as HTMLHeadingElement;
        this.headerPointsField = document.getElementById(`task-${idNum}-header-points`) as HTMLParagraphElement;
    }

    disconnectedCallback() {}

    getAvailableIdNum() {
        const idStart = "task-editor-";

        let i = 0;
        while (document.getElementById(`${idStart}${i}`) != null) {
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

        if (this.titleFiField) {
            this.titleFiField.value = textFi?.title ?? "";
        }
        if (this.titleEnField) {
            this.titleEnField.value = textEn?.title ?? "";
        }
        if (this.bodyFiField) {
            this.bodyFiField.value = textFi?.body ?? "";
        }
        if (this.bodyEnField) {
            this.bodyEnField.value = textEn?.body ?? "";
        }
        if (this.pointsField) {
            this.pointsField.value = data.points.toString();
        }
        if (this.headerTitleField) {
            this.headerTitleField.innerText = textFi?.title ?? "Uusi tehtävä";
        }
        if (this.headerPointsField) {
            this.headerPointsField.innerText = `${data.points}p`;
        }
    }

    addEventListeners() {
        const header = document.getElementById(`task-${this.idNumber}-header`);
        header?.addEventListener("click", (_) => {
            // Change content visibility
            const content = document.getElementById(`task-${this.idNumber}-content`) as HTMLElement;
            const isOpen = content.style.display === "block";
            content.style.display = isOpen ? "none" : "block";

            // Flip header opened icon
            const isOpenedImg = document.getElementById(`task-${this.idNumber}-opened-img`);
            isOpenedImg?.classList.toggle("rotate-180deg");
        });

        const deleteBtn = document.getElementById(`delete-task-${this.idNumber}-btn`);
        deleteBtn?.addEventListener("click", (event) => {
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
    let editors = document.getElementsByClassName("task-header");
    for (let i = 0; i < editors.length; i++) {
        if (editors[i] instanceof TaskEditor) {
            (editors[i] as TaskEditor).addEventListeners();
        }
    }
}
