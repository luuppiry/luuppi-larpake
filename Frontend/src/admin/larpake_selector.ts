import LarpakeClient from "../api_client/larpake_client.ts";
import LarpakePreview from "../components/larpake-previewer.ts";

async function main() {
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
            year: larpake.year,
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
