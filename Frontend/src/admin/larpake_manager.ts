import SectionEditor, { SectionData } from "../components/section-editor";

document.getElementById("commonCancelBtn")?.addEventListener("click", (_) => {
    // Cancel to default values
});

document.getElementById("commonSaveBtn")?.addEventListener("click", (_) => {
    // Validate and send new values to server
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
