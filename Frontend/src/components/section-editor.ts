const idStart = "section-editor-";

export type TaskData = {
    id: number;
    titleFi: string;
    titleEn: string;
};

export default class SectionEditor extends HTMLElement {
    idNumber: number | null = null;
    titleFiField: HTMLInputElement | null = null;
    titleEnField: HTMLInputElement | null = null;
    serverTaskId: number | null = null;

    constructor() {
        super();
    }

    connectedCallback() {
        this.idNumber = this.getAvailableIdNum();
        const idNum = this.idNumber;

        this.innerHTML = `
            <li>
                <div class="larpake-section">
                    <h3>Osio</h3>
                    <div class="section-divider">
                        <div class="field">
                            <label for="section-${idNum}-title-fi">Otsikko (fi, oletus)</label>
                            <input
                                id="section-${idNum}-title-fi"
                                class="text-field"
                                placeholder="Tanpereella"
                                maxlength="80"
                            />
                        </div>
                        <div class="field">
                            <label for="section-${idNum}-title-en">Otsikko (en)</label>
                            <input
                                id="section-${idNum}-title-en"
                                class="text-field"
                                placeholder="In Tampere"
                                maxlength="80"
                            />
                        </div>
                    </div>
                    <ol class="section-list task-list">
                        <!-- Tasks in this section -->
                    </ol>
                </div>
            </li>
            `;

        this.id = `${idStart}${idNum}`;
        this.titleFiField = document.getElementById(`section-${idNum}-title-fi`) as HTMLInputElement;
        this.titleEnField = document.getElementById(`section-${idNum}-title-en`) as HTMLInputElement;
    }

    disconnectedCallback() {}

    getAvailableIdNum() {
        let i = 0;
        while (document.getElementById(`${idStart}${i}`) != null) {
            if (i > 1000) {
                console.log(`Low on available section editor ids, currently searching ${i}`);
            }
            i++;
        }
        return i;
    }

    appendTask(task: HTMLElement) {
        this.appendChild(task);
    }

    removeTask(task: HTMLElement) {
        this.removeChild(task);
    }

    setData(data: TaskData) {
        this.serverTaskId = data.id;

        if (this.titleFiField) {
            this.titleFiField.value = data.titleFi;
        }
        if (this.titleEnField) {
            this.titleEnField.value = data.titleEn;
        }
    }
}

if ("customElements" in window) {
    customElements.define("section-editor", SectionEditor);
}

export function func() {
    console.log("func")
}