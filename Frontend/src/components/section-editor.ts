import TaskEditor, { TaskData } from "./task-editor";

const idStart = "section-editor-";

export type SectionData = {
    id: number;
    titleFi: string;
    titleEn: string;
    tasks: TaskData[];
};

export default class SectionEditor extends HTMLElement {
    idNumber: number | null = null;
    titleFiField: HTMLInputElement | null = null;
    titleEnField: HTMLInputElement | null = null;
    serverTaskId: number | null = null;
    tasksContainer: HTMLOListElement | null = null;

    constructor() {
        super();
    }

    connectedCallback() {
        this.idNumber = this.#getAvailableIdNum();
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
                    <ol id="section-${idNum}-tasks-container" class="section-list task-list">
                        <!-- Tasks in this section -->
                    </ol>
                    <div id="add-task-${idNum}-btn" class="add-task">
                        <img src="/icons/plus_icon.svg" alt="add new icon">
                    </div>
                </div>
            </li>
            `;

        this.id = `${idStart}${idNum}`;
        this.titleFiField = document.getElementById(`section-${idNum}-title-fi`) as HTMLInputElement;
        this.titleEnField = document.getElementById(`section-${idNum}-title-en`) as HTMLInputElement;
        this.tasksContainer = document.getElementById(`section-${idNum}-tasks-container`) as HTMLOListElement;

        document.getElementById(`add-task-${idNum}-btn`)?.addEventListener("click", (_) => {
            this.appendTask({
                id: -1,
                titleFi: "Uusi Tehtävä",
                titleEn: "",
                bodyFi: "",
                bodyEn: "",
                points: 0,
            });
        });
    }

    disconnectedCallback() {}

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

    appendTask(task: TaskData) {
        const editor = new TaskEditor();
        this.tasksContainer?.appendChild(editor);
        editor.setData(task);
        editor.addEventListeners();
    }

    removeTask(task: HTMLElement) {
        this.tasksContainer?.removeChild(task);
    }

    setData(data: SectionData) {
        this.serverTaskId = data.id;

        if (this.titleFiField) {
            this.titleFiField.value = data.titleFi;
        }
        if (this.titleEnField) {
            this.titleEnField.value = data.titleEn;
        }

        data.tasks.forEach((task) => this.appendTask(task));
    }
}

if ("customElements" in window) {
    customElements.define("section-editor", SectionEditor);
}
