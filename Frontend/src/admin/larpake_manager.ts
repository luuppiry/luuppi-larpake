import SectionEditor, { SectionData } from "../components/section-editor.ts";
import mapChildren, { isEmpty } from "../helpers.ts";

type CommonData = {
    startYear: number | null;
    titleFi: string;
    descriptionFi: string;
    titleEn: string;
    descriptionEn: string;
};

function render() {
    const data: SectionData[] = [
        {
            id: 0,
            titleFi: "Testi",
            titleEn: "Test",
            tasks: [
                {
                    id: 0,
                    titleFi: "Tehtävä 1",
                    titleEn: "Task 1",
                    bodyFi: "Tehtävän kuvaus",
                    bodyEn: "Task description",
                    points: 5,
                },
                {
                    id: 1,
                    titleFi: "Tehtävä 2",
                    titleEn: "Task 2",
                    bodyFi: "Tehtävän kuvaus",
                    bodyEn: "Task description",
                    points: 10,
                },
            ],
        },
        {
            id: 0,
            titleFi: "Testi 2",
            titleEn: "Test 2",
            tasks: [
                {
                    id: 0,
                    titleFi: "Tehtävä 1",
                    titleEn: "Task 1",
                    bodyFi: "Tehtävän kuvaus",
                    bodyEn: "Task description",
                    points: 2,
                },
                {
                    id: 1,
                    titleFi: "Tehtävä 2",
                    titleEn: "Task 2",
                    bodyFi: "Tehtävän kuvaus",
                    bodyEn: "Task description",
                    points: 7,
                },
            ],
        },
    ];

    const container = document.getElementById("section-container");
    if (container == null) {
        throw new Error("Section container is null.");
    }

    const url = window.location.href;
    const query = url.split("?");
    if (query.length > 1) {
        const params = new URLSearchParams(query[1]);
        const larpakeId = params.get("larpakeId");
        if (larpakeId != null) {
            // Set existing data to UI
            data.forEach((sectionData) => {
                const editor = new SectionEditor();
                container?.appendChild(editor);
                editor.setData(sectionData);
            });
        }
    }
}

function addPageEventListeners() {
    document.getElementById("common-info-cancel-btn")?.addEventListener("click", (event) => {
        event.preventDefault();

        updatePageIfOk();
    });

    document.getElementById("common-info-submit-btn")?.addEventListener("click", (event) => {
        event.preventDefault();

        // Validate and send new values to server

        const data = readCommonData();

        console.log(data);
        const dialog = document.getElementById("common-data-saved-dialog") as HTMLDialogElement;
        dialog.showModal();
        dialog.querySelector("._close-btn")?.addEventListener("click", (_) => {
            dialog.close();
        });
    });

    document.getElementById("tasks-cancel-btn")?.addEventListener("click", (event) => {
        event.preventDefault();

        updatePageIfOk();
    });

    document.getElementById("tasks-submit-btn")?.addEventListener("click", (event) => {
        event.preventDefault();

        const data = readSectionData();
        if (data == null){
            return;
        }
        console.log(data);

        /* Upload section/task data here */

        const dialog = document.getElementById("tasks-saved-dialog") as HTMLDialogElement;
        dialog.showModal();
        dialog.querySelector("._close-btn")?.addEventListener("click", (_) => {
            dialog.close();
        });
    });

    document.getElementById("add-section-btn")?.addEventListener("click", (event) => {
        event.preventDefault();

        const container = document.getElementById("section-container");
        if (container == null) {
            throw new Error("Could not find section container, check naming.");
        }

        container.appendChild(new SectionEditor());
    });
}

function updatePageIfOk() {
    const dialog = document.getElementById("are-you-sure-dialog") as HTMLDialogElement;
    dialog.showModal();

    dialog.querySelector("._cancel")?.addEventListener("click", (_) => {
        dialog.close();
    });
    dialog.querySelector("._ok")?.addEventListener("click", (_) => {
        window.location.href = window.location.href;
    });
}

function showInvalidDataDialog() {
    const dialog = document.getElementById("invalid-data-dialog") as HTMLDialogElement;
    dialog.showModal();
    dialog.querySelector("._close-btn")?.addEventListener("click", (_) => {
        dialog.close();
    });
}

function readSectionData(): SectionData[] | null {
    const container = document.getElementById("section-container") as HTMLOListElement;
    if (container == null) {
        throw new Error("Section container is null");
    }

    try {
        return mapChildren<SectionData>(container.children, (elem) => {
            if (elem instanceof SectionEditor) {
                return (elem as SectionEditor).getData();
            }
            return null;
        });
    } catch (ex) {
        console.log("You are probably looking for the next error below:");
        console.error(ex);
        showInvalidDataDialog();
        return null;
    }
}

function readCommonData(): CommonData {
    const startYearInput = document.getElementById("start-year") as HTMLInputElement;
    const titleFi = document.getElementById("title-fi") as HTMLInputElement;
    const titleEn = document.getElementById("title-en") as HTMLInputElement;
    const descFi = document.getElementById("description-fi") as HTMLTextAreaElement;
    const descEn = document.getElementById("description-en") as HTMLTextAreaElement;

    if (isEmpty(titleFi.value)) {
        showInvalidDataDialog();
        throw new Error("Lärpäke title (fi) cannot be null");
    }
    if (isEmpty(titleFi.value)) {
        showInvalidDataDialog();
        throw new Error("Lärpäke title (en) cannot be null");
    }

    const startYear = parseInt(startYearInput.value);
    return {
        startYear: Number.isNaN(startYear) ? null : startYear,
        titleFi: titleFi.value,
        descriptionFi: descFi.value,
        titleEn: titleEn.value,
        descriptionEn: descEn.value,
    };
}

function main() {
    addPageEventListeners();
    render();
}
main();
