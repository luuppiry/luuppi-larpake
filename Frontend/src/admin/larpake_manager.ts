import LarpakeClient from "../api_client/larpake_client.ts";
import SectionEditor from "../components/section-editor.ts";
import mapChildren, { getInputNumericByDocId, isEmpty, LANG_EN, LANG_FI, SectionSortFunc, ToDictionary } from "../helpers.ts";
import { Larpake, Section } from "../models/larpake.ts";

const client = new LarpakeClient();

async function main() {
    addPageEventListeners();
    await render();
}

const container = document.getElementById("section-container") as HTMLOListElement;

async function render() {
    const container = document.getElementById("section-container");
    if (container == null) {
        throw new Error("Section container is null.");
    }

    // Read query for
    const url = window.location.href;
    const query = url.split("?");
    if (query.length > 1) {
        const params = new URLSearchParams(query[1]);
        const larpakeId = parseInt(params.get("larpakeId") ?? "");
        if (!Number.isNaN(larpakeId)) {
            // Loadable larpake found, load and show it
            await loadExternal(larpakeId);
        }
    } else {
        // No loadable data, open new editor
        window.location.href = `${url}?new=true`;
    }
}

async function loadExternal(larpakeId: number): Promise<void> {
    const idField = document.getElementById("id-field") as HTMLInputElement;
    idField.value = larpakeId.toString();

    const larpake = await client.getById(larpakeId);
    if (larpake == null) {
        throw new Error("Failed to fetch Larpake tasks.");
    }

    const records = await client.getTasksByLarpakeId(larpakeId);
    if (records == null) {
        throw new Error("Failed to fetch Larpake tasks.");
    }

    // Map tasks
    const tasks = ToDictionary(records, (task) => task.larpakeSectionId);

    setCommonLarpakeData(larpake);

    // Map sections and render
    larpake.sections?.sort(SectionSortFunc).forEach((section) => {
        const editor = new SectionEditor();
        container?.appendChild(editor);
        editor.setData(section, tasks.get(section.id) ?? []);
    });
}

function setCommonLarpakeData(larpake: Larpake) {
    const fi = larpake.textData.filter((x) => x.languageCode == LANG_FI)[0];
    const en = larpake.textData.filter((x) => x.languageCode == LANG_EN)[0];

    const startYear = document.getElementById("start-year") as HTMLInputElement;
    startYear.value = larpake.year?.toString() ?? "";

    const titleFi = document.getElementById("title-fi") as HTMLInputElement;
    titleFi.value = fi?.title ?? "";

    const descFi = document.getElementById("description-fi") as HTMLTextAreaElement;
    descFi.value = fi?.description ?? "";

    const titleEn = document.getElementById("title-en") as HTMLInputElement;
    titleEn.value = en?.title ?? "";

    const descEn = document.getElementById("description-en") as HTMLTextAreaElement;
    descEn.value = en?.description ?? "";
}

function addPageEventListeners() {
    document.getElementById("common-info-cancel-btn")?.addEventListener("click", (event) => {
        event.preventDefault();

        confirmAndRefreshPage();
    });

    document.getElementById("common-info-submit-btn")?.addEventListener("click", async (event) => {
        event.preventDefault();

        // Validate and send new values to server

        const data: Larpake = readCommonData();

        console.log(data);
        await client.uploadLarpakeCommonData(data);        
        
        const dialog = document.getElementById("common-data-saved-dialog") as HTMLDialogElement;
        dialog.showModal();
        dialog.querySelector("._close-btn")?.addEventListener("click", (_) => {
            dialog.close();
        });
    });

    document.getElementById("tasks-cancel-btn")?.addEventListener("click", (event) => {
        event.preventDefault();

        confirmAndRefreshPage();
    });

    document.getElementById("tasks-submit-btn")?.addEventListener("click", (event) => {
        event.preventDefault();

        const data = readSectionData();
        if (data == null) {
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

function confirmAndRefreshPage() {
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

function readSectionData(): Section[] | null {
    const container = document.getElementById("section-container") as HTMLOListElement;
    if (container == null) {
        throw new Error("Section container is null");
    }

    try {
        return mapChildren<Section>(container.children, (elem) => {
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

function readCommonData(): Larpake {
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

    const id = getInputNumericByDocId("id-field");
    const startYear = getInputNumericByDocId("start-year");

    // year: Number.isNaN(startYear) ? null : startYear,
    // titleFi: titleFi.value,
    // descriptionFi: descFi.value,
    // titleEn: titleEn.value,
    // descriptionEn: descEn.value,

    return {
        id: Number.isNaN(id) ? -1 : id,
        year: Number.isNaN(startYear) ? null : startYear,
        createdAt: new Date(),
        updatedAt: new Date(),
        sections: null,
        textData: [
            {
                title: titleFi!.value,
                description: descFi?.value ?? "",
                languageCode: LANG_FI,
            },
            {
                title: titleEn!.value,
                description: descEn?.value ?? "",
                languageCode: LANG_EN,
            },
        ],
    };
}



main();
