import AttendanceClient from "./api_client/attendance_client.js";
import LarpakeClient from "./api_client/larpake_client.js";
import { parseAttendanceLink } from "./builders.js";
import { Q_TASK_ID, SERVER_STATUSES } from "./constants.js";
import { formatDateTime, getMatchingLangObject, getSearchParams } from "./helpers.js";
import { AttendanceKey } from "./models/attendance.js";
import { LarpakeTask, LarpakeTaskTextData, Section, SectionTextData } from "./models/larpake.js";

const taskClient = new LarpakeClient();
const attendanceClient = new AttendanceClient(taskClient.client);
// TODO: task completed SSE event

async function main() {
    const params = getSearchParams();
    const taskId = parseInt(params.get(Q_TASK_ID) ?? "");

    if (Number.isNaN(taskId)) {
        redirectNotFound();
        return;
    }

    const task = await taskClient.getTaskById(taskId);
    if (!task) {
        redirectNotFound();
        return;
    }

    // Get parent section of the task
    const section = await taskClient.getSectionById(task.larpakeSectionId);

    // Get key if attendance was found
    const completionKey = await attendanceClient.getAttendanceByTaskId(task.id);
    if (Number.isInteger(completionKey)) {
        if (completionKey == SERVER_STATUSES.USER_STATUS_TUTOR) {
            console.log("No QR-code, because user is not competing.");
        } else {
            alert("Something went wrong whilst trying to fetch completion key!");
        }
    }

    const key = completionKey as AttendanceKey;
    render(task, key, section);
}

function redirectNotFound() {
    window.location.href = "404.html";
}

function render(task: LarpakeTask, completionKey: AttendanceKey | null, section: Section | null) {
    const container = document.getElementById("event-container") as HTMLElement;

    const taskText = getMatchingLangObject<LarpakeTaskTextData>(task.textData);
    const sectionText = getMatchingLangObject<SectionTextData>(section?.textData ?? null);

    container.querySelector<HTMLHeadingElement>("._name")!.innerText = sectionText?.title
        ? `/ ${sectionText?.title}`
        : "";
    container.querySelector<HTMLHeadingElement>("._title")!.innerText = taskText?.title ?? "N/A";
    container.querySelector<HTMLParagraphElement>("._updated")!.innerText = formatDateTime(
        task.updatedAt
    );
    container.querySelector<HTMLParagraphElement>("._description")!.innerText =
        taskText?.body ?? "";

    renderQRCode(completionKey);
}

function renderQRCode(completionKey: AttendanceKey | null) {
    const container = document.getElementById("qr-container") as HTMLElement;

    const link = parseAttendanceLink(completionKey?.qrCodeKey ?? null);
    if (!link) {
        console.warn("Failed to produce attendance completion link.");
        return;
    }

    const params = new URLSearchParams();
    params.set("size", "250x250");
    params.set("data", link);

    // https://api.qrserver.com/v1/create-qr-code/?data=${event.code}&amp;size=250x250
    const url = `https://api.qrserver.com/v1/create-qr-code/?${params.toString()}`;
    container.querySelector<HTMLImageElement>("._qr-code")!.src = url;

    const keyField = container.querySelector<HTMLParagraphElement>("._code")!;
    keyField.innerText = completionKey?.key ?? "N/A";
    keyField.addEventListener("click", () => {
        if (completionKey?.key) {
            navigator.clipboard.writeText(completionKey?.key);
        }
    });
}

main();
