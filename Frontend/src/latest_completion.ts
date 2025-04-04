import AttendanceClient from "./api_client/attendance_client.js";
import LarpakeClient from "./api_client/larpake_client.js";
import {
    appendTemplateElement,
    formatDateTime,
    getDocumentLangCode,
    getMatchingLangObject,
    LANG_EN,
    ToOverwriteDictionary,
} from "./helpers.js";
import { Attendance } from "./models/attendance.js";
import { LarpakeTask, LarpakeTaskTextData } from "./models/larpake.js";

const larpakeClient = new LarpakeClient();
const attendanceClient = new AttendanceClient(larpakeClient.client);

async function main() {
    const container = document.getElementById("attendance-container") as HTMLOListElement;
    if (!container) {
        throw new Error("Attendance container not found, check naming");
    }

    let attendances = await attendanceClient.getAll(null, true, true, 20);
    if (!attendances) {
        throw new Error("Failed to fetch attendances.");
    }

    // Filter uncompleted to be sure
    attendances = attendances.filter((x) => x.completed != null);
    const taskIds = attendances?.filter((x) => x.larpakeTaskId).map((x) => x.larpakeTaskId);
    const tasks = await larpakeClient.getTasks(null, taskIds);
    if (!tasks) {
        console.warn("Failed to load any tasks, ids shown instead.");
    }

    const lookup = ToOverwriteDictionary(tasks ?? [], (x) => x.id);
    render(container, attendances, lookup);
}

function render(
    container: HTMLOListElement,
    attendances: Attendance[],
    tasks: Map<number, LarpakeTask>
) {
    const lang = getDocumentLangCode();

    for (const attendance of attendances
        .filter((x) => x.completed)
        .sort((first, second) => {
            // Converting to date (to be sure) must be very unefficient, but I don't care here, its max 30 items
            return new Date(first.completed!.completedAt) > new Date(second.completed!.completedAt)
                ? -1
                : 1;
        })) {
        if (!attendance.completed) {
            continue;
        }
        // Create item
        const elem = appendTemplateElement<HTMLElement>("attendance-template", container);

        // Format data
        const task = tasks.get(attendance.larpakeTaskId) ?? null;
        const time = parseTime(attendance);
        const title = parseTitle(attendance, task, lang);

        // Render data
        elem.querySelector<HTMLHeadingElement>("._title")!.innerText = title;
        elem.querySelector<HTMLHeadingElement>("._time")!.innerText = time;
    }
}

function parseTitle(attendance: Attendance, task: LarpakeTask | null, lang: string): string {
    // Use id or generic name if null
    const defaultTitle =
        attendance.larpakeTaskId.toString() ?? lang === LANG_EN ? "Task" : "Tehtävä";

    if (!task) {
        return defaultTitle;
    }
    let text = getMatchingLangObject<LarpakeTaskTextData>(task.textData, lang);

    const title = text?.title ? text.title : defaultTitle;
    return `${title} (${task.points}P)`;
}

function parseTime(attendance: Attendance | null): string {
    if (attendance?.completed?.completedAt) {
        return formatDateTime(attendance.completed.completedAt);
    }
    return "N/A";
}

main();
