import AttendanceClient from "./api_client/attendance_client.js";
import LarpakeClient from "./api_client/larpake_client.js";
import { parseAttendanceLink } from "./builders.js";
import { Q_TASK_ID, SERVER_STATUS } from "./constants.js";
import { formatDateTime, getMatchingLangObject, getSearchParams } from "./helpers.js";
import { AttendanceKey } from "./models/attendance.js";
import { LarpakeTask, LarpakeTaskTextData, Section, SectionTextData } from "./models/larpake.js";
import ClickerService from "./services/clicker_service.js";

const taskClient = new LarpakeClient();
const attendanceClient = new AttendanceClient(taskClient.client);

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
        if (completionKey == SERVER_STATUS.USER_STATUS_TUTOR) {
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

async function render(task: LarpakeTask, completionKey: AttendanceKey | null, section: Section | null) {
    const container = document.getElementById("event-container") as HTMLElement;

    // Removing all loader animations
    removeLoaderClasses();

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

    // Checking whether task is already done and displaying information if so
    let attendances = await attendanceClient.getAll(null, true, true, 20);
    console.log(attendances);
    let taskCompleted = false;
    if(attendances != null){
        taskCompleted = attendances.some(item => item.larpakeTaskId === task.id);
        if (taskCompleted) {
            const container = document.getElementById("qr-container") as HTMLElement;
            const foundTask = attendances.find(item => item.larpakeTaskId === task.id);
            const formattedTime = formatLocalTime(foundTask.completed.completedAt);
            container.innerHTML = `<h1>Onneksi olkoon!</h1><p>Tehtävä on suoritettu ajassa: ${formattedTime}</p>
            <img id="award-icon" src="/icons/award-icon.png" alt="Suoritettu jee jee"/>`;
        }
    }

    if (!task.cancelledAt) {
        renderAttendanceCodes(completionKey);
    } else {
        const label = document.getElementById("cancelled-label") as HTMLParagraphElement;
        label.classList.remove("hidden");
    }
}

function formatLocalTime(isoString) {
    const date = new Date(isoString);
    const pad = (num) => String(num).padStart(2, '0');
  
    const hours = pad(date.getHours());
    const minutes = pad(date.getMinutes());
    const day = pad(date.getDate());
    const month = pad(date.getMonth() + 1);
    const year = date.getFullYear();
  
    return `${hours}:${minutes} ${day}.${month}.${year}`;
}

function removeLoaderClasses() {
    document.querySelectorAll(".loader").forEach((el) => {
        el.classList.remove("loader");
    });
} 

function renderAttendanceCodes(completionKey: AttendanceKey | null) {
    const container = document.getElementById("qr-container") as HTMLElement;

    const link = parseAttendanceLink(completionKey?.qrCodeKey ?? null);
    if (!link) {
        console.warn("Failed to produce attendance completion link.");
        return;
    }

    const params = new URLSearchParams();
    params.set("size", "250x250");
    params.set("data", link);

    // Example URL
    // https://api.qrserver.com/v1/create-qr-code/?data=${event.code}&amp;size=250x250
    const url = `https://api.qrserver.com/v1/create-qr-code/?${params.toString()}`;

    const qrCodeImage = container.querySelector<HTMLImageElement>("._qr-code")!;
    const notFoundImage = container.querySelector<HTMLImageElement>("._not-found")!;

    const resetQrCode = () => {
        qrCodeImage.src = "/qr-placeholder.png";
    };

    qrCodeImage.src = url;
    if (url) {
        // Add click actions
        new ClickerService(qrCodeImage, resetQrCode);
        notFoundImage.style.display = "none";
    }

    // Add key field copy to clickboard functionality
    const keyField = container.querySelector<HTMLParagraphElement>("._code")!;
    keyField.innerText = completionKey?.key ?? "N/A";
    keyField.addEventListener("click", () => {
        if (completionKey?.key) {
            const copiedLabel = container.querySelector<HTMLElement>("._copied")!;
            navigator.clipboard.writeText(completionKey?.key).then((_) => {
                copiedLabel.style.display = "block";
                copiedLabel.style.opacity = "1";
                setTimeout(() => {
                    copiedLabel.style.opacity = "0";
                    setTimeout(() => {
                        copiedLabel.style.display = "none";
                    }, 1000);
                }, 800);
            });
        }
    });
}

main();
