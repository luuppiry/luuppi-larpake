export type TaskData = {
    id: number;
    titleFi: string;
    titleEn: string;
    bodyFi: string;
    bodyEn: string;
    points: number;
};

export default class TaskEditor extends HTMLElement {
    titleFiField: HTMLInputElement | null = null;
    titleEnField: HTMLInputElement | null = null;
    bodyFiField: HTMLTextAreaElement | null = null;
    bodyEnField: HTMLTextAreaElement | null = null;
    pointsField: HTMLInputElement | null = null;
    idNumber: number | null = null;
    serverTaskId: number | null = null;

    constructor() {
        super();
    }

    connectedCallback() {
        this.idNumber = this.getAvailableIdNum();
        const idNum = this.idNumber;

        const header = `
            <div class="task-header">
                <div>
                    <h4>Title</h4>
                    <p>&HorizontalLine;</p>
                    <p>Xp</p>
                </div>
                <img src="/icons/menu_closed.svg" alt="menu closed icon" />
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
                        class="text-field start-year-field"
                        type="number"
                        min="1"
                        max="100"
                        step="1"
                        value="3"
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
                            class="text-field"
                            placeholder="Kaupunkikävely"
                            maxlength="80"
                        />
                    </div>
                    <div class="field">
                        <label for="task-${idNum}-body-fi">Selitys</label>
                        <textarea
                            id="task-${idNum}-body-fi"
                            class="description-field text-field"
                            placeholder="kuvaus"
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
                            class="text-field"
                            placeholder="Kaupunkikävely"
                            maxlength="80"
                        />
                    </div>
                    <div class="field">
                        <label for="task-${idNum}-body-en">Selitys</label>
                        <textarea
                            id="task-${idNum}-body-en"
                            class="description-field text-field"
                            placeholder="body"
                            maxlength="800"
                        ></textarea>
                    </div>
                </div>
            </div>
            `;

        const content = `
            <div class="task-content">
                ${commonInfo}
                ${localizedInfo}
            </div>
            `;

        this.innerHTML = `
        <li class="task">
            ${header}
            ${content}
        </li>
        `;

        this.id = `task-editor-${idNum}`;
        this.titleFiField = document.getElementById(`task-${idNum}-title-fi`) as HTMLInputElement;
        this.titleEnField = document.getElementById(`task-${idNum}-title-en`) as HTMLInputElement;
        this.bodyFiField = document.getElementById(`task-${idNum}-body-fi`) as HTMLTextAreaElement;
        this.bodyEnField = document.getElementById(`task-${idNum}-body-en`) as HTMLTextAreaElement;
        this.pointsField = document.getElementById(`task-${idNum}-body-en`) as HTMLInputElement;
    }

    disconnectedCallback() {}

    getAvailableIdNum() {
        const idStart = "task-editor-";

        let i = 0;
        while (document.getElementById(`${idStart}${i}`) != null) {
            if (i > 200) {
                console.log(`Low on available task editor ids, currently searching ${i}`);
            }
            i++;
        }
        return i;
    }

    setData(data: TaskData) {
        this.serverTaskId = data.id;

        if (this.titleFiField) {
            this.titleFiField.value = data.titleFi;
        }
        if (this.titleEnField) {
            this.titleEnField.value = data.titleEn;
        }
        if (this.bodyFiField) {
            this.bodyFiField.value = data.bodyFi;
        }
        if (this.bodyEnField) {
            this.bodyEnField.value = data.bodyEn;
        }
        if (this.pointsField) {
            this.pointsField.value = data.points.toString();
        }
    }

    addAllEventListeners(){
        addTaskEventListeners();
    }
}

if ("customElements" in window) {
    customElements.define("task-editor", TaskEditor);
}

export function addTaskEventListeners() {
    let collapsibles = document.getElementsByClassName("task-header");

    for (let i = 0; i < collapsibles.length; i++) {
        const current = collapsibles[i];
        current.addEventListener("click", (_) => {
            const content = current.nextElementSibling as HTMLElement;
            const isOpen = content.style.display === "block";

            content.style.display = isOpen ? "none" : "block";
            for (let j = 0; j < current.children.length; j++) {
                const currentChild = current.children.item(j);
                if (currentChild instanceof HTMLImageElement) {
                    currentChild.classList.toggle("rotate-180deg");
                }
            }
        });
    }
}
