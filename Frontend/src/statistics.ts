import AttendanceClient from "./api_client/attendance_client.js";
import LarpakeClient from "./api_client/larpake_client.js";
import { Larpake, LarpakeTask } from "./models/larpake.js";
import { ToDictionary } from "./helpers.js";
import { Q_LARPAKE_ID, Q_LAST_PAGE, Q_OF_PAGES, Q_PAGE } from "./constants.js";
import { appendTemplateElement, getSearchParams, removeChildren } from "./helpers.js";

const client = new LarpakeClient();
const attendanceClient = new AttendanceClient(client.client);

type Statistic = {
    larpakeSectionId: number;
    title: string;
    totalPoints: number;
    earnedPoints: number;
};

async function main() {
    const params = getSearchParams();
    const larpakeId = params.get(Q_LARPAKE_ID);

    const container = document.getElementById("statistics-container") as HTMLUListElement;

    const statistics = await fetchStatistics(parseInt(larpakeId ?? ""));
    if (statistics.length === 0) {
        return;
    }

    removeChildren(container);

    const totalEarnedPoints = statistics.reduce((sum, s) => sum + s.earnedPoints, 0);
    const totalPointsAllSections = statistics.reduce((sum, s) => sum + s.totalPoints, 0);
    statistics.forEach((statistic) => {
        const elem = appendTemplateElement<HTMLLIElement>("statistics-value", container);
        elem.querySelector<HTMLSpanElement>("._title")!.innerText = statistic.title;
        elem.querySelector<HTMLSpanElement>("._value")!.innerText = statistic.earnedPoints.toString();
        elem.querySelector<HTMLSpanElement>("._max")!.innerText = statistic.totalPoints.toString();
    });

    // Metadata
    const totalPoinsHolder = document.getElementById("totalPointsHolder") as HTMLDivElement;
    removeChildren(totalPoinsHolder);
    totalPoinsHolder.append(`${totalEarnedPoints} / ${totalPointsAllSections}`);

    const page = parseInt(params.get(Q_PAGE) ?? "0");
    const maxPage = parseInt(params.get(Q_OF_PAGES) ?? "0");

    const pageInfo = document.getElementById("page-info") as HTMLParagraphElement;
    pageInfo.innerText = `${page} / ${maxPage}`;

    const backButton = document.getElementById("back-btn") as HTMLAnchorElement;
    const returnParams = new URLSearchParams();
    returnParams.append(Q_LARPAKE_ID, larpakeId?.toString() ?? "-1");
    returnParams.append(Q_LAST_PAGE, "true");

    backButton.href = `larpake.html?${returnParams.toString()}`;
}

async function fetchStatistics(larpakeId: number): Promise<Statistic[]> {
    const larpake = await getLarpake(larpakeId);
    await addSectionTasks(larpake);
    const attendances = (await attendanceClient.getAll(larpake.id)) ?? [];
    const completedTaskIds = new Set(attendances.map(a => a.larpakeTaskId));
    const data: Statistic[] = (larpake.sections ?? []).map(section => {
      const totalPoints = section.tasks?.reduce((sum, task) => sum + (task.points || 0), 0) ?? 0;
      const sectionName = section.textData?.find(t => t.languageCode === document.documentElement.lang)?.title || 'Untitled Section';
      const earnedPoints =
        section.tasks?.reduce((sum, task) => {
          return completedTaskIds.has(task.id) ? sum + (task.points || 0) : sum;
        }, 0) ?? 0;
  
      return {
        larpakeSectionId: section.id,
        title: sectionName,
        totalPoints,
        earnedPoints
      };
    });
    return data;
}  

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

async function addSectionTasks(larpake: Larpake) {
    const unmappedTasks = (await client.getTasksByLarpakeId(larpake.id)) ?? [];
    const tasks = ToDictionary<number, LarpakeTask>(unmappedTasks, (x) => x.larpakeSectionId);
    for (const section of larpake.sections ?? []) {
        section.tasks = tasks.get(section.id) ?? [];
    }
}

main();
