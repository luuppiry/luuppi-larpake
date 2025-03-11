import SectionEditor, { SectionData } from "../components/section-editor";

document.getElementById("common-info-cancel-btn")?.addEventListener("click", (event) => {
    event.preventDefault();

    updatePageIfOk();
});

document.getElementById("common-info-submit-btn")?.addEventListener("click", (event) => {
    event.preventDefault();

    // Validate and send new values to server

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


function updatePageIfOk(){
    const dialog = document.getElementById("are-you-sure-dialog") as HTMLDialogElement;
    dialog.showModal();

    dialog.querySelector("._cancel")?.addEventListener("click", (_) => {
        dialog.close();
    });
    dialog.querySelector("._ok")?.addEventListener("click", (_) => {
        window.location.href = window.location.href;
    });
}

render();
