import LarpakeClient from "../api_client/larpake_client.ts";
import LarpakePreview, { PreviewData } from "../components/larpake-previewer.ts";

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

async function main() {
    // data.forEach((larpake) => {
    //     const item = new LarpakePreview();
    //     container.appendChild(item);
    //     item.addData(larpake);
    // });

    const client = new LarpakeClient();
    const larpakkeet = await client.getAll(false);

    if (larpakkeet == null) {
        return;
    }


    larpakkeet!.forEach((larpake) => {
        const item = new LarpakePreview();
        container.appendChild(item);

        item.addData({
            id: larpake.id,
            titleFi: larpake.textData[0].title,
            year: null,
            sections: larpake.sections?.length ?? -1,
            tasks: -1,
            points: -1,
            groups: -1,
            createdAt: larpake.createdAt,
            updatedAt: larpake.updatedAt,
            imgUrl: "/kiasa.png",
        });
    });
}

const container = document.getElementById("larpake-container") as HTMLUListElement;

main();
