import AttendanceClient from "./api_client/attendance_client.js";
import LarpakeClient from "./api_client/larpake_client.js";
import {
    appendTemplateElement,
    formatDateTime,
    getDocumentLangCode,
    isEmpty,
    LANG_EN,
    ToOverwriteDictionary,
} from "./helpers.js";
import { Attendance } from "./models/attendance.js";
import { LarpakeTask } from "./models/larpake.js";

const larpakeClient = new LarpakeClient();
const attendanceClient = new AttendanceClient(larpakeClient.client);

async function main() {
    const container = document.getElementById("attendance-container") as HTMLOListElement;
    if (!container) {
        throw new Error("Attendance container not found, check naming");
    }

    let attendances = await attendanceClient.get(null, true, true, 30);
    if (!attendances) {
        throw new Error("Failed to fetch attendances.");
    }

    // Filter uncompleted to be sure
    attendances = attendances.filter((x) => x.completed != null);
    const taskIds = attendances?.filter((x) => x.larpakeTaskId).map((x) => x.larpakeTaskId);
    const tasks = await larpakeClient.getTasks(null, taskIds);
    if (!tasks) {
        console.warn("Failed to load any tasks, ids shown instead.");
        renderSimple(container, attendances);
        return;
    }

    const lookup = ToOverwriteDictionary(tasks, (x) => x.id);
    render(container, attendances, lookup);
}

function renderSimple(container: HTMLOListElement, attendances: Attendance[]) {
    const lang = getDocumentLangCode();

    for (const attendance of attendances) {
        const elem = appendTemplateElement<HTMLElement>("attendance-container", container);
        renderIdOnlyItem(elem, attendance.larpakeTaskId, attendance.completed!.completedAt!, lang);
    }
}

function render(container: HTMLOListElement, attendances: Attendance[], tasks: Map<number, LarpakeTask>) {
    const lang = getDocumentLangCode();

    for (const attendance of attendances) {
        // Create item
        const elem = appendTemplateElement<HTMLElement>("attendance-container", container);

        const title = tasks.get(attendance.larpakeTaskId);
        if (title) {
            elem.querySelector<HTMLHeadingElement>("._title");
        } else {
            renderIdOnlyItem(elem, attendance.larpakeTaskId, attendance.completed!.createdAt, lang);
        }

        const completed = formatDateTime(attendance.completed?.completedAt!);

        elem.querySelector<HTMLParagraphElement>("._time")!.innerText = isEmpty(completed) ? "N/A" : completed;
    }
}

function renderIdOnlyItem(elem: HTMLElement, taskId: number, completedAt: Date, lang: string = "fi") {
    elem.querySelector<HTMLHeadingElement>("._title")!.innerText = `${
        lang === LANG_EN ? "Task" : "Tehtävä"
    } ${taskId} `;

    const completed = formatDateTime(completedAt);
    elem.querySelector<HTMLParagraphElement>("._time")!.innerText = isEmpty(completed) ? "N/A" : completed;
}

main();
