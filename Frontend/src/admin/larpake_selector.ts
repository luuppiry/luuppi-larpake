import LarpakePreview, { PreviewData } from "../components/larpake-previewer";

const data: PreviewData[] = [
    {
        id: 1,
        titleFi: "Kiasan Seikkailu Lärpäke",
        year: 2024,
        sections: 4,
        tasks: 35,
        points: 120,
        groups: 12,
        createdAt: new Date(2024, 2, 20, 12, 30),
        updatedAt: new Date(2024, 2, 24, 0, 23),
        imgUrl: "/kiasa.png",
    },
    {
        id: 2,
        titleFi: "Fuksi Ankan Taskulärpäke",
        year: 2023,
        sections: 5,
        tasks: 43,
        points: 170,
        groups: 14,
        createdAt: new Date(2023, 1, 12, 15, 8),
        updatedAt: new Date(2023, 7, 15, 12, 13),
        imgUrl: "/donald_duck.png",
    },
];

const container = document.getElementById("larpake-container") as HTMLUListElement;

data.forEach((larpake) => {
    const item = new LarpakePreview();
    container.appendChild(item);
    item.addData(larpake);
});
