import { Q_TASK_ID, Q_LARPAKE_ID, Q_LAST_PAGE, Q_OF_PAGES, Q_PAGE } from "../constants.js";
import {
    appendTemplateElement,
    isEmpty,
    LANG_EN,
    removeChildren,
    throwIfAnyNull,
    ToOverwriteDictionary,
} from "../helpers.js";
import { Attendance, Completion } from "../models/attendance.js";
import { getSectionText, getTaskText, Section } from "../models/larpake.js";
import { Signature } from "../models/user.js";
import { SectionSortFunc, TaskSortFunc } from "../sortFunctions.js";
import SignatureRenderer from "./signature_renderer.js";

const PAGE_SIZE = 6;
const TASK_LINE_LENGTH = 55;

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
class Filler { }

type Page = {
    header: string;
    tasks: (Task | Title | Filler)[];
};

export class LarpakeRenderer {
    currentPage: number = 0;
    header: HTMLHeadingElement;
    taskContainer: HTMLUListElement;
    indexer: HTMLParagraphElement;
    nextBtn: HTMLButtonElement;
    previousBtn: HTMLButtonElement;
    pages: Page[];
    signatures: Map<string, Signature>;
    larpakeId: number | null = null;
    language: string;

    constructor(
        header: HTMLHeadingElement,
        container: HTMLUListElement,
        indexer: HTMLParagraphElement,
        nextBtn: HTMLButtonElement,
        prevBtn: HTMLButtonElement,
        lang: string
    ) {
        throwIfAnyNull([header, container, indexer, nextBtn, prevBtn]);

        this.header = header;
        this.taskContainer = container;
        this.indexer = indexer;
        this.nextBtn = nextBtn;
        this.previousBtn = prevBtn;
        this.language = lang;
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
        this.setPage(this.pages.length - 1);
    }

    setup(sections: Section[], attendances: Attendance[], signatures: Signature[]) {
        this.larpakeId = sections[0].larpakeId;
        this.pages = [];
        this.signatures = ToOverwriteDictionary(signatures, (x) => x.id);

        const completions = new Map<number, Completion>();
        for (const attendance of attendances) {
            if (attendance.completed) {
                completions.set(attendance.larpakeTaskId, attendance.completed);
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
            window.location.href =
                `statistics.html?${Q_LARPAKE_ID}=${this.larpakeId}&${Q_LAST_PAGE}=true` +
                `&${Q_PAGE}=${this.currentPage + 1}&${Q_OF_PAGES}=${this.currentPage + 1}`;
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
            window.location.href = `task_info.html?${Q_TASK_ID}=${task.id}&${Q_LARPAKE_ID}=${this.larpakeId}`;
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
        const state = this.language === LANG_EN ? "CANCELLED" : "PERUUTETTU";
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

    #updatePageQuery() {
        const url = new URL(window.location.href);
        const params = new URLSearchParams(url.search);
        params.set(Q_LARPAKE_ID, this.larpakeId?.toString() ?? "-1");
        params.set(Q_PAGE, this.currentPage.toString());
        url.search = params.toString();
        window.history.pushState({}, "", url);
    }
}
