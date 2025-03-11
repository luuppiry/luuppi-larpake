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
        this.serverSectionId = data.id;

        if (this.titleFiField) {
            this.titleFiField.value = data.titleFi;
        }
        if (this.titleEnField) {
            this.titleEnField.value = data.titleEn;
        }

        data.tasks.forEach((task) => this.appendTask(task));
    }

    getData(): SectionData {
        // Validate field values exist
        if (this.titleFiField?.value == undefined) {
            throw new Error("Section title (fi) cannot be empty.");
        }
        if (this.titleEnField?.value == undefined) {
            throw new Error("Section title (en) cannot be empty.");
        }

        const container = this.tasksContainer;
        if (container == null) {
            throw new Error("Task container is null");
        }

        // Parse tasks
        const tasks: TaskData[] = [];
        for (let i = 0; i < container.children.length; i++) {
            const editor = container.children.item(i);
            if (editor instanceof TaskEditor) {
                const task = (editor as TaskEditor).getData();
                tasks.push(task);
            }
        }

        return {
            id: this.serverSectionId ?? -1,
            titleFi: this.titleFiField.value,
            titleEn: this.titleEnField.value,
            tasks: tasks,
        };
    }
}

if ("customElements" in window) {
    customElements.define("section-editor", SectionEditor);
}
