import { isEmpty, LANG_EN, LANG_FI } from "../helpers";
import { LarpakeTask, Section } from "../models/larpake";
import TaskEditor from "./task-editor";

const idStart = "section-editor-";

export default class SectionEditor extends HTMLElement {
    idNumber: number | null = null;
    titleFiField: HTMLInputElement | null = null;
    titleEnField: HTMLInputElement | null = null;
    serverSectionId: number | null = null;
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
                                name="section-title-fi"
                                class="text-field"
                                placeholder="Osion otsikko"
                                maxlength="80"
                                minlenght="3"
                                required
                            />
                        </div>
                        <div class="field">
                            <label for="section-${idNum}-title-en">Otsikko (en)</label>
                            <input
                                id="section-${idNum}-title-en"
                                name="section-title-en"
                                class="text-field"
                                placeholder="Section title"
                                maxlength="80"
                                minlenght="3"
                                required
                            />
                        </div>
                    </div>
                    <ol id="section-${idNum}-tasks-container" class="section-list task-list">
                        <!-- Tasks in this section -->
                    </ol>
                    <button id="add-task-${idNum}-btn" class="add-task add-btn">
                        <img src="/icons/plus_icon.svg" alt="add new icon">
                    </button>
                </div>
            </li>
            `;

        this.id = `${idStart}${idNum}`;
        this.titleFiField = document.getElementById(`section-${idNum}-title-fi`) as HTMLInputElement;
        this.titleEnField = document.getElementById(`section-${idNum}-title-en`) as HTMLInputElement;
        this.tasksContainer = document.getElementById(`section-${idNum}-tasks-container`) as HTMLOListElement;

        document.getElementById(`add-task-${idNum}-btn`)?.addEventListener("click", (event) => {
            event.preventDefault();
            this.appendTask({
                id: 0,
                larpakeSectionId: 0,
                points: 0,
                orderingWeightNumber: 0,
                cancelledAt: null,
                createdAt: new Date(),
                updatedAt: new Date(),
                textData: [],
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

    appendTask(task: LarpakeTask) {
        const editor = new TaskEditor();
        this.tasksContainer?.appendChild(editor);
        editor.setData(task);
        editor.addEventListeners();
    }

    removeTask(task: HTMLElement) {
        this.tasksContainer?.removeChild(task);
    }

    setData(data: Section, tasks: LarpakeTask[]) {
        const textFi = data.textData.filter((x) => x.languageCode == LANG_FI)[0];
        const textEn = data.textData.filter((x) => x.languageCode == LANG_EN)[0];

        this.serverSectionId = data.id;

        if (this.titleFiField) {
            this.titleFiField.value = textFi?.title ?? "";
        }
        if (this.titleEnField) {
            this.titleEnField.value = textEn?.title ?? "";
        }

        tasks.forEach((task) => this.appendTask(task));
    }

    getData(): Section {
        // Validate field values exist
        if (isEmpty(this.titleFiField?.value)) {
            throw new Error("Section title (fi) cannot be empty.");
        }
        if (isEmpty(this.titleEnField?.value)) {
            throw new Error("Section title (en) cannot be empty.");
        }

        const container = this.tasksContainer;
        if (container == null) {
            throw new Error("Task container is null");
        }

        // Parse tasks
        const tasks: LarpakeTask[] = [];
        for (let i = 0; i < container.children.length; i++) {
            const editor = container.children.item(i);
            if (editor instanceof TaskEditor) {
                const task = (editor as TaskEditor).getData();
                task.larpakeSectionId = this.serverSectionId ?? -1;
                tasks.push(task);
            }
        }
        return {
            id: this.serverSectionId ?? -1,
            larpakeId: -1,
            orderingweightNumber: 0,
            createdAt: new Date(),
            updatedAt: new Date(),
            textData: [
                {
                    title: this.titleFiField!.value,
                    languageCode: LANG_FI,
                },
                {
                    title: this.titleEnField!.value,
                    languageCode: LANG_EN,
                },
            ],
        tasks: tasks
        };
    }
}

if ("customElements" in window) {
    customElements.define("section-editor", SectionEditor);
}
