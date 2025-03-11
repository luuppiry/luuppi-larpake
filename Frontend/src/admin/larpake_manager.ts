import SectionEditor, { SectionData } from "../components/section-editor";

document.getElementById("common-info-cancel-btn")?.addEventListener("click", (event) => {
    event.preventDefault();

    const path = window.location.href.split("?")[0];
    window.location.href = path;
});

document.getElementById("common-info-submit-btn")?.addEventListener("click", (event) => {
    event.preventDefault();

    // Validate and send new values to server
});

document.getElementById("tasks-cancel-btn")?.addEventListener("click", (event) => {
    event.preventDefault();

    const path = window.location.href.split("?")[0];
    window.location.href = path;
});

document.getElementById("tasks-submit-btn")?.addEventListener("click", (event) => {
    event.preventDefault();
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

    data.forEach((sectionData) => {
        const editor = new SectionEditor();
        container?.appendChild(editor);
        editor.setData(sectionData);
    });
}

render();
