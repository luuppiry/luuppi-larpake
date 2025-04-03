import LarpakeClient from "./api_client/larpake_client.js";
import { Q_TASK_ID } from "./constants.js";
import { formatDateTime, getDocumentLangCode, getMatchingLangObject, getSearchParams } from "./helpers.js";
import { LarpakeTask, LarpakeTaskTextData, Section, SectionTextData } from "./models/larpake.js";

const client = new LarpakeClient();

// TODO: task completed SSE event

async function main() {
  const params = getSearchParams();
  const taskId = parseInt(params.get(Q_TASK_ID) ?? "");

  if (!Number.isNaN(taskId)) {
    redirectNotFound();
    return;
  }

  const task = await client.getTaskById(taskId);
  if (!task) {
    redirectNotFound();
    return;
  }
  const section = await client.getSectionById(taskId);

  render(task, section);

}

function redirectNotFound() {
  window.location.href = "404.html";
}


function render(task: LarpakeTask, section: Section | null) {
  const container = document.getElementById("event-container") as HTMLElement;

  const taskText = getMatchingLangObject<LarpakeTaskTextData>(task.textData);
  const sectionText = getMatchingLangObject<SectionTextData>(section?.textData ?? null)

  container.querySelector<HTMLHeadingElement>("._name")!.innerText = sectionText?.title ? `/ ${sectionText?.title}` : "";
  container.querySelector<HTMLHeadingElement>("._title")!.innerText = taskText?.title ?? "N/A"
  container.querySelector<HTMLParagraphElement>("._updated")!.innerText = formatDateTime(task.updatedAt);
  container.querySelector<HTMLParagraphElement>("._description")!.innerText = taskText?.body ?? "";

}





main();