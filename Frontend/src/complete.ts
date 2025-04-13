import AttendanceClient from "./api_client/attendance_client.js";
import { removeAttendanceCodeHeader as removeAttendanceCodeHeader } from "./builders.js";
import { Q_ATTENDANCE_KEY } from "./constants.js";
import {
    getDocumentLangCode,
    getMatchingLangObject,
    getSearchParams,
    LANG_EN,
    redirect404Page,
} from "./helpers.js";
import { FatAttendance } from "./models/attendance.js";
import { LarpakeTaskTextData } from "./models/larpake.js";

const attendanceClient = new AttendanceClient();

async function main() {
    document.getElementById("cancel-btn")?.addEventListener("click", _ => {
        window.location.href = "read.html"
    })




    const params = getSearchParams();
    const rawKey = params.get(Q_ATTENDANCE_KEY);
    if (!rawKey) {
        redirect404Page(Q_ATTENDANCE_KEY);
        return;
    }

    const key = removeAttendanceCodeHeader(rawKey ?? "");
    if (!key) {
        alertNotFound(null);
        return;
    }

    const attendance = await attendanceClient.getAttendanceByKey(key);
    if (!attendance) {
        alertNotFound(key);
        return;
    }

    const complete = async () => {
        const id = await attendanceClient.completeKeyed(key);
        if (typeof id !== "string") {
            const lang = getDocumentLangCode();
            alert(
                lang !== LANG_EN
                    ? `Toiminto epäonnistui: ${id.message} (Virhekoodi: ${id.applicationError})`
                    : `Completion failed: ${id.message} (Error: ${id.applicationError})`
            );
            return;
        }
        showSuccess(id);
    };

    render(attendance, complete);
}

function render(attendance: FatAttendance, complete: () => void) {
    const user = attendance.user;

    const taskText = getMatchingLangObject<LarpakeTaskTextData>(attendance.task.textData);

    const section = document.getElementById("info") as HTMLElement;
    section.querySelector<HTMLElement>("._first-name")!.innerText = user.firstName ?? "N/A";
    section.querySelector<HTMLElement>("._last-name")!.innerText = user.lastName ?? "N/A";
    section.querySelector<HTMLElement>("._username")!.innerText = user.username ?? "N/A";
    section.querySelector<HTMLElement>("._task")!.innerText = taskText?.title ?? "N/A";
    section.querySelector<HTMLElement>("._points")!.innerText = attendance.task.points.toString();
    const submitBtn = section.querySelector<HTMLButtonElement>("._submit")!;
    submitBtn.addEventListener("click", complete);

    const isCompleted = attendance.completed != null;
    submitBtn.disabled = isCompleted;
    if (isCompleted) {
        section.querySelector<HTMLElement>("._completed")!.classList.remove("hidden");
    }
}

function alertNotFound(key: string | null) {
    const lang = getDocumentLangCode();
    const isFinnish = lang !== LANG_EN;

    if (!key) {
        alert(
            isFinnish
                ? "Toiminto epäonnistui, suoritusavain väärässä formaatissa."
                : "Failed to process, attendance key in invalid format."
        );
        return;
    }

    alert(
        isFinnish
            ? `Avainta '${key}' ei löydy tietokannasta, se voi olla vanhentunut.`
            : `Key '${key}' not found on database, it might be expired.`
    );
}

function showSuccess(id: string) {
    const target = "read.html";

    const dialog = document.getElementById("success-dialog") as HTMLDialogElement;
    dialog.showModal();

    dialog.querySelector<HTMLSpanElement>("._id")!.innerText = id;
    dialog.querySelector<HTMLElement>("._ok")?.addEventListener("click", (_) => {
        window.location.href = target;
    });

    setInterval(() => {
        window.location.href = target;
    }, 2000);
}

main();
