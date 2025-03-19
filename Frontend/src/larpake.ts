import AttendanceClient from "./api_client/attendance_client";
import LarpakeClient from "./api_client/larpake_client";
import {
    appendTemplateElement,
    getDocumentLangCode,
    isEmpty,
    LANG_EN,
    removeChildren,
    SectionSortFunc,
    TaskSortFunc,
    throwIfAnyNull,
    ToDictionary,
    ToOverwriteDictionary,
} from "./helpers";
import { Attendance, Completion as Completion } from "./models/attendance";
import { getSectionText, getTaskText, Larpake, LarpakeTask, Section } from "./models/larpake";
import { Signature } from "./models/user";
import SignatureRenderer from "./services/signature_renderer";

const PAGE_SIZE = 7;
const TASK_LINE_LENGTH = 55;

// Query parameters
const Q_LARPAKE_ID = "LarpakeId";
const Q_PAGE = "Page";
const Q_EVENT_ID = "EventId";
const Q_LAST_PAGE = "LastPage";

class Title {
    title: string;

    constructor(title: string) {
        this.title = title;
    }
}

class Task {
    id: number;
    title: string;
    isCancelled: boolean;
    isCompleted: boolean;
    signatureId: string | null;
    points: number;

    constructor(
        id: number,
        title: string,
        isCancelled: boolean,
        isCompleted: boolean,
        points: number,
        signatureId: string | null
    ) {
        this.id = id;
        this.title = title;
        this.isCancelled = isCancelled;
        this.isCompleted = isCompleted;
        this.signatureId = signatureId;
        this.points = points;
    }
}
class Filler {}

type Page = {
    header: string;
    tasks: (Task | Title | Filler)[];
};

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

    // Load tasks
    const unmappedTasks = (await client.getTasksByLarpakeId(larpake.id)) ?? [];
    const tasks = ToDictionary<number, LarpakeTask>(unmappedTasks, (x) => x.larpakeSectionId);
    for (const section of larpake.sections ?? []) {
        section.tasks = tasks.get(section.id) ?? [];
    }

    // Load attendances
    const attendances = (await attendanceClient.get(larpake.id)) ?? [];

    // Load signatures
    const signatureIds: string[] = attendances
        .filter((x) => x.completed?.signatureId != null)
        .map((x) => x.completed!.signatureId!);
    const signatures = await attendanceClient.getSignatures(signatureIds);

    // Render
    const renderer = new PageRenderer();
    renderer.setup(larpake.sections ?? [], attendances, signatures);
    if (!Number.isNaN(pageNum) && !isLastPage) {
        renderer.setPage(pageNum);
    }
    if (isLastPage) {
        renderer.goToLastPage();
    }
    renderer.render();
}

class PageRenderer {
    currentPage: number = 0;
    header: HTMLHeadingElement;
    taskContainer: HTMLUListElement;
    indexer: HTMLParagraphElement;
    nextBtn: HTMLButtonElement;
    previousBtn: HTMLButtonElement;
    pages: Page[];
    signatures: Map<string, Signature>;
    larpakeId: number | null = null;

    constructor() {
        this.header = document.getElementById("larpake-page-name") as HTMLHeadingElement;
        this.taskContainer = document.getElementById("larpake-task-container") as HTMLUListElement;
        this.indexer = document.getElementById("page-info") as HTMLParagraphElement;
        this.nextBtn = document.getElementById("next-page") as HTMLButtonElement;
        this.previousBtn = document.getElementById("prev-page") as HTMLButtonElement;
        this.pages = [];
        this.signatures = new Map<string, Signature>();

        throwIfAnyNull([this.header, this.taskContainer, this.indexer, this.nextBtn, this.previousBtn]);

        this.previousBtn.addEventListener("click", (_) => {
            this.#changePage(-1);
        });

        this.nextBtn.addEventListener("click", (_) => {
            this.#changePage(1);
        });
    }

    setPage(pageId: number | null) {
        if (pageId) {
            this.currentPage = pageId <= this.pages.length ? pageId : this.pages.length;
        }
    }

    goToLastPage() {
        this.setPage(this.pages.length);
    }

    setup(sections: Section[], attendances: Attendance[], signatures: Signature[]) {
        this.larpakeId = sections[0].larpakeId;
        this.pages = [];
        this.signatures = ToOverwriteDictionary(signatures, (x) => x.id);

        const completions = new Map<number, Completion>();
        for (const attendance of attendances) {
            if (attendance.completed) {
                completions.set(attendance.larpakeEventId, attendance.completed);
            }
        }

        const pieces = sections.sort(SectionSortFunc).flatMap((s) => {
            const sectionTitle = getSectionText(s).title;
            const tasks = s.tasks.sort(TaskSortFunc).map((task) => {
                const completion = completions.get(task.id);
                return new Task(
                    task.id,
                    getTaskText(task).title,
                    task.cancelledAt != null,
                    completion != undefined,
                    task.points,
                    completion?.signatureId ?? null
                );
            });

            return [new Title(sectionTitle), ...tasks];
        });

        const formatTitle = (title: string) => {
            return isEmpty(title) ? "Lärpäke" : `Lärpäke / ${title}`;
        };

        const pushFinalizedPage = (page: Page, lastTitle: string) => {
            if (isEmpty(page.header)) {
                page.header = lastTitle;
            }
            this.pages.push(page);
        };

        const createNewPage = (tasks: (Task | Title | Filler)[] | null = null): Page => {
            const title = tasks?.find((x) => x instanceof Title);
            return { header: title?.title ?? "", tasks: tasks ?? [] };
        };

        const isPageFull = (page: Page): boolean => {
            return page.tasks.length >= PAGE_SIZE;
        };

        // Iterate over tasks and fill pages
        let lastTitle: string = "Lärpäke";
        let page: Page = { header: "", tasks: [] };

        const pushTask = (elem: Task | Title) => {
            page.tasks.push(elem);

            // Nice guard clauses
            const titleOversize = elem.title.length > TASK_LINE_LENGTH;
            if (!titleOversize) {
                return;
            }

            if (isPageFull(page)) {
                return;
            }

            // Maximum of 2 fillers
            const fillerCount = page.tasks.filter((x) => x instanceof Filler).length;
            const containsMaxFillers = fillerCount >= 2;
            if (containsMaxFillers) {
                return;
            }

            // If 2 title, only 1 filler
            const titleCount = page.tasks.filter((x) => x instanceof Title).length;
            if (fillerCount !== 0 && titleCount >= 2) {
                return;
            }

            /* Filler is not rendered.
             * I only makes room if multi line tasks. */
            page.tasks.push(new Filler());
        };

        pieces.forEach((piece) => {
            // Add page title if no title already exists
            if (piece instanceof Title) {
                lastTitle = formatTitle(piece.title);
                if (isEmpty(page.header)) {
                    page.header = lastTitle;
                }
            }

            // Append full page
            if (isPageFull(page)) {
                if (page.tasks[-1] instanceof Title) {
                    const moveTitle = page.tasks.pop()!;
                    pushFinalizedPage(page, lastTitle);
                    page = createNewPage([moveTitle]);
                } else {
                    pushFinalizedPage(page, lastTitle);
                    page = createNewPage();
                }
            }
            pushTask(piece);
        });

        // Add partially empty page
        if (page.tasks.length > 0) {
            pushFinalizedPage(page, lastTitle);
        }
    }

    render() {
        if (this.currentPage >= this.pages.length && this.pages.length !== 0) {
            window.location.href = `statistics.html?${Q_LARPAKE_ID}=${this.larpakeId}&${Q_LAST_PAGE}=true`;
            return;
        }

        this.#updatePageQuery();

        // Update header
        this.header.innerText = this.pages[this.currentPage]?.header ?? "Error loading title";

        // Update pagination info
        this.indexer.innerText = `${this.currentPage + 1} / ${this.pages.length + 1}`;

        // Update button states
        this.previousBtn.disabled = this.currentPage === 0;

        // Update buttons
        removeChildren(this.taskContainer);

        const elements = this.pages[this.currentPage]?.tasks ?? [];
        for (const elem of elements) {
            switch (true) {
                case elem instanceof Task:
                    this.#appendTask(elem as Task);
                    break;
                case elem instanceof Filler:
                    // No need to act
                    break;
                default:
                    this.#appendTitle(elem as Title);
                    break;
            }
        }
    }

    #appendTask(task: Task) {
        const element = appendTemplateElement<HTMLElement>("task-template", this.taskContainer);

        element.querySelector<HTMLHeadingElement>("._title")!.innerText = task.title;
        element.querySelector<HTMLHeadingElement>("._points")!.innerText = `${task.points ?? 0}P`;
        element.addEventListener("click", (_) => {
            window.location.href = `event_marking.html?${Q_EVENT_ID}=${task.id}&${Q_LARPAKE_ID}=${this.larpakeId}`;
        });

        if (task.isCancelled) {
            this.#taskStateCancelled(element);
            return;
        }

        const signatureContainer = element.querySelector<HTMLDivElement>("._signature")!;
        /* Default task state is completed, so it should be only
         * Overwritten if task was completed with signature or
         * task was cancelled. */
        if (task.isCompleted) {
            this.#taskStateCompleted(task, signatureContainer);
        } else {
            removeChildren(signatureContainer);
        }
    }

    #taskStateCompleted(task: Task, container: HTMLDivElement) {
        const signature = task.signatureId ? this.signatures.get(task.signatureId) : null;
        if (signature) {
            removeChildren(container);
            const data = signature.signature;
            const sign = new SignatureRenderer(data.data, {
                stroke: data.strokeStyle,
                fill: "black",
                strokeWidth: data.lineWidth,
                strokeLinecap: data.lineCap,
            });
            sign.compile();
            sign.renderTo(container);
        }
    }

    #taskStateCancelled(elem: HTMLElement) {
        const state = getDocumentLangCode() === LANG_EN ? "CANCELLED" : "PERUUTETTU";
        elem.querySelector<HTMLHeadingElement>("._state")!.innerText = state;

        const btn = elem.querySelector<HTMLElement>("._btn")!;
        btn.classList.add("cancelled");
        btn.classList.remove("hover-scale-03");
    }

    #appendTitle(title: Title) {
        const element = appendTemplateElement<HTMLElement>("title-template", this.taskContainer);
        element.querySelector<HTMLHeadingElement>("._title")!.innerText = title.title;
    }

    #changePage(step: number) {
        // Check if going forwards button is disabled
        if (step > 0 && this.nextBtn.disabled) {
            return;
        }

        // Check if going backwords button is disabled
        if (step < 0 && this.previousBtn.disabled) {
            return;
        }

        this.currentPage += step;
        this.render();
    }

    #updatePageQuery(){
        const url = new URL(window.location.href);
        const params = new URLSearchParams(url.search)
        params.set(Q_PAGE, this.currentPage.toString())
        url.search = params.toString();
        window.history.pushState({}, "", url)
    }
}

main();
