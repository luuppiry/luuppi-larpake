import AttendanceClient from "./api_client/attendance_client.js";
import LarpakeClient from "./api_client/larpake_client.js";

import { Larpake, LarpakeTask } from "./models/larpake.js";
import { Q_LARPAKE_ID, Q_LAST_PAGE, Q_PAGE } from "./constants.js";
import { LarpakeRenderer } from "./services/larpake_render.js";
import { getDocumentLangCode, ToDictionary } from "./helpers.js";

const client = new LarpakeClient();
const attendanceClient = new AttendanceClient(client.client);

async function getLarpake(larpakeId: number): Promise<Larpake> {
    if (!Number.isNaN(larpakeId)) {
        const result = await client.getById(larpakeId, false);
        if (result) {
            return result;
        }
    }
    const available = await client.getOwn();
    if (!available) {
        throw new Error("Could not load any larpake from server.");
    }
    return available![0];
}

async function main() {
    // Parse query string to get page number
    const urlParams = new URLSearchParams(window.location.search);
    const larpakeId: number = parseInt(urlParams.get(Q_LARPAKE_ID) ?? "");
    const pageNum: number = parseInt(urlParams.get(Q_PAGE) ?? "0");
    const isLastPage: boolean = urlParams.get(Q_LAST_PAGE)?.toLowerCase() === "true";

    // Load larpake and sections
    const larpake = await getLarpake(larpakeId);
    if (larpake == undefined){
        const taskContainer = document.getElementById("larpake-task-container") as HTMLUListElement;
        const lang = getDocumentLangCode();
        const translations = {
            fi: {
                emptyTitle: "Tyhjää täynnä!",
                emptyDesc1: "Et ole osallistunut vielä yhteenkään Lärpäkkeeseen.",
                emptyDesc2: "Jos kuitenkin tiedät olevasi merkattu osallistujana Lärpäkkeeseen, saattaa vika olla palvelimella tai nettiyhteydessä. Jos ongelma toistuu, ota yhteyttä ylläpitoon.",
            },
            en: {
                emptyTitle: "Full of emptiness!",
                emptyDesc1: "You have not yet attended any Lärpäke.",
                emptyDesc2: "If you know you to be attending some Lärpäke, the problem might lie somewhere on our servers or internet connection. If this keeps happening, contact system administration.",
            },
        };

        const t = translations[lang]; // Pick the right language
        taskContainer.innerHTML = `
            <section class="container">
                <h2>${t.emptyTitle}</h2>
                <p>${t.emptyDesc1}</p>
                <p>${t.emptyDesc2}</p>
            </section>
        `;
        return;
    }

    // Load tasks
    await addSectionTasks(larpake);

    // Load attendances
    const attendances = (await attendanceClient.getAll(larpake.id)) ?? [];

    // Load signatures
    const signatureIds: string[] = attendances
        .filter((x) => x.completed?.signatureId != null)
        .map((x) => x.completed!.signatureId!);
    const signatures = await attendanceClient.getSignatures(signatureIds);

    // Loading components
    const header = document.getElementById("larpake-page-name") as HTMLHeadingElement;
    const taskContainer = document.getElementById("larpake-task-container") as HTMLUListElement;
    const indexer = document.getElementById("page-info") as HTMLParagraphElement;
    const nextBtn = document.getElementById("next-page") as HTMLButtonElement;
    const previousBtn = document.getElementById("prev-page") as HTMLButtonElement;

    // Render
    const lang = getDocumentLangCode();
    const renderer = new LarpakeRenderer(header, taskContainer, indexer, nextBtn, previousBtn, lang);

    renderer.setup(larpake.sections ?? [], attendances, signatures);

    if (!Number.isNaN(pageNum) && !isLastPage) {
        renderer.setPage(pageNum);
    }
    if (isLastPage) {
        renderer.goToLastPage();
    }
    renderer.render();
}

main();
async function addSectionTasks(larpake: Larpake) {
    const unmappedTasks = (await client.getTasksByLarpakeId(larpake.id)) ?? [];
    const tasks = ToDictionary<number, LarpakeTask>(unmappedTasks, (x) => x.larpakeSectionId);
    for (const section of larpake.sections ?? []) {
        section.tasks = tasks.get(section.id) ?? [];
    }
}
