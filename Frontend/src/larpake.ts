import AttendanceClient from "./api_client/attendance_client";
import LarpakeClient from "./api_client/larpake_client";
import {
    appendTemplateElement,
    removeChildren,
    SectionSortFunc,
    TaskSortFunc,
    throwIfAnyNull,
    ToDictionary,
} from "./helpers";
import { Attendance, Completion as Completion } from "./models/attendance";
import { getSectionText, getTaskText, Larpake, LarpakeTask,  Section } from "./models/larpake";

// Parse query string to get page number

const PAGE_SIZE = 6;

const pages = [
    {
        header: "Lärpäke / Ensi askeleet",
        buttons: [
            "ENSI ASKELEET - OTSIKKO",
            "TUTUSTUMISILTA 19.8 5P",
            "KAUPUNKIKÄVELY 3P",
            "TUTUSTUMISSAUNA 20.8 5P",
            "PUISTOHENGAILU 3P - PERUUTETTU",
            "KAMPUSKIERROS 3P",
        ],
    },
    {
        header: "Lärpäke / Ensi askeleet",
        buttons: [
            "KÄY PÄÄAINEESI ALOITUSLUENNOLLA 2P",
            "OSALLISTU TREY:N FUKSISUUNNISTUKSEEN 5P",
            "LÄRPÄKKEEN PÄÄLLYSTYS TAI KORISTELU 2P",
            "KERÄÄ VIIDELTÄ TUUTORILTA ALLEKIRJOITUS 5P",
            "FUKSIRYHMÄN TAPAHTUMA 5P",
            "KUMMIRYHMÄN TAPAHTUMA 5P",
        ],
    },
    {
        header: "Lärpäke / Ensi askeleet",
        buttons: [
            "PUBIRUNDI 3P",
            "LAULA KARAOKESSA 2P",
            "FUKSIKEILAUS 4P",
            "PIENEN PIENI LUUPPILAINEN - OTSIKKO",
            "LIITY LUUPIN JÄSENEKSI 20P",
            "TILAA HAALARIT 10P",
        ],
    },
    {
        header: "Lärpäke / Pienen pieni luuppilainen",
        buttons: [
            "OSTA LUUPIN HAALARIMERKKI 2P",
            "JUTTELE KYYKÄSTÄ DDRNV:N JÄSENEN KANSSA TAI OSTA HAALARIMERKKI 2P",
            "OSTA LUUPPI-TUOTE (pl. Luuppi-haalarimekki) 3P",
            "OMPELE 5 HAALARIMERKKIÄ 6P",
            "OSALLISTU KOPO-TAPAHTUMAAN 5P",
            "KIERTOAJELU 5P",
        ],
    },
    {
        header: "Lärpäke / Pii-Klubilla tapahtuu",
        buttons: [
            "KASTAJAISET 5P",
            "PII-KLUBILLA TAPAHTUU - OTSIKKO",
            "KÄY TOIMISTOLLA 2P",
            "KEITÄ PANNULLINEN KAHVIA 3P",
            "PULLAPÄIVÄ 2P",
            "OSALLISTU TALKOOPÄIVÄÄN 3P",
        ],
    },
    {
        header: "Lärpäke / Normipäivä",
        buttons: [
            "NORMIPÄIVÄ - OTSIKKO",
            "ENNAKKOLIPPU KOLMIOILLE 3P",
            "OSALLISTU KOLMIOIDEN VIRALLISILLE ETKOILLE 26.9 3P",
            "KOTIBILEET/ETKOT (OMAT/LUUPPILAISEN) 2P",
            "SITSIT 3P",
            "PELI-ILTA 3P",
        ],
    },
    {
        header: "Lärpäke / Normipäivä",
        buttons: [
            "LAUTAPELI-ILTA 3P",
            "LANIT 2P",
            "MUU TAPAHTUMA 5P",
            "YLIOPISTOELÄMÄÄ - OTSIKKO",
            "SELVITÄ KOULUTUSASIANTUNTIJASI NIMI 2P",
            "LUO OMA HOPS 5P",
        ],
    },
    {
        header: "Lärpäke / Yliopistoelämää",
        buttons: [
            "SIVUAINEPANEELI 5P",
            "MTT-TIE-KAHVIT 3P",
            "KÄY LASKUTUVASSA TAI LUUPIN OPINTOPIIRISSÄ 3P",
            "KÄY SYÖMÄSSÄ 4 ERI OPISKELIJARAVINTOLASSA 4P",
            "VIERAILE TOISEN AINEJÄRJESTÖN/KILLAN/KERHON TILOISSA 2P",
            "HANKI TOISEN TOISEN AINEJÄRJESTÖN/KILLAN/KERHON HAALARIMERKKI 2P",
        ],
    },
    {
        header: "Lärpäke / Yliopistoelämää",
        buttons: [
            "OSALLISTU YLIOPISTON TUTKIMUKSEEN 6P",
            "VAIKUTUSVALTAA - OTSIKKO",
            "KYSY SPONSORIA HAALAREIHIN 5P",
            "SAA SPONSORI HAALAREIHIN 15P",
            "NAKKEILE TAPAHTUMASSA 5P",
            "OSALLISTU LUUPIN HALLITUKSEN KOKOUKSEN 5P",
        ],
    },
    {
        header: "Lärpäke / Vaikutusvaltaa",
        buttons: [
            "JUTTU LUUPPISANOMIIN 7P",
            "LUUPIN HAALARIMERKKIKISA 2P",
            "OSALLISTU HALLITUSINFO-TAPAHTUMAAN 5P",
            "OSALLISTU HALLITUSPANEELLIIN 3P",
            "SÄÄNTÖMÄÄRÄINEN VUOSIKOKOUS 5P",
            "ÄÄNESTÄ TREY:N EDUSTAJISTOVAALEISSA 2P",
        ],
    },
    {
        header: "Lärpäke / Liikunnallista",
        buttons: [
            "LIIKUNNALLISTA - OTSIKKO",
            "FUKSIKYYKKÄ (ALKULOHKOT) 5P",
            "FUKSIKYYKKÄ (FINAALI) 5P",
            "LUUPIN LIIKUNTAVUORO 3P",
            "LASERTAISTELU 3P",
            "METSÄRETKI 3P",
        ],
    },
    {
        header: "Lärpäke / Kaikenlaista",
        buttons: [
            "LIIKUNTAHAASTE 3P",
            "MUU LIIKUNTATAPAHTUMA 3P",
            "KAIKENLAISTA - OTSIKKO",
            "TEK-INFO 2P",
            "LOIMU-INFO 2P",
            "SEURAA LUUPIN SOMEA (TG/IG/TT) 2P",
        ],
    },
    {
        header: "Lärpäke / Kaikenlaista",
        buttons: [
            "OSALLISTU ATK-YTP:LLE 5P",
            "OSALLISTU INTEGRAATIOFESTEILLE 5P",
            "KULTTUURITAPAHTUMA 4P",
            "POIKKITIETEELLINEN TAPAHTUMA TAMPEREELLA 3P",
            "LUUPIN PELITURNAUS 3P",
            "YRITYSTAPAHTUMA 3P",
        ],
    },
    {
        header: "Lärpäke / Tanpereella",
        buttons: [
            "KESKUSTAEXCU 3P",
            "SELVITÄ LUUPIN KIASA-NORSUN TAUSTATARINA 1P",
            "TANPEREELLA - OTSIKKO",
            "KÄY PYYNIKIN NÄKÖTORNILLA 2P",
            "KÄY HERVANNAN VESITORNILLA 2P",
            "SYÖ SIIPIÄ, VEGESIIPIÄ, MUSTAMAKKARAA TAI PYYNIKIN MUNKKEJA 2P",
        ],
    },
    {
        header: "Lärpäke / Tanpereella",
        buttons: [
            "KÄY MUSEOSSA TAMPEREELLA 2P",
            "KÄY HOTELLI TORNIN HUIPULLA 2P",
            "KÄY NÄSINNEULALLA 2P",
            "RATIKKA-AJELU 5P",
            "KÄY JOKAISELLA YLIOPISTON KAMPUKSELLA 3P",
            "- TYHJÄ",
        ],
    },
];

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
    signatureId: string | null;

    constructor(id: number, title: string, isCancelled: boolean, signatureId: string | null) {
        this.id = id;
        this.title = title;
        this.isCancelled = isCancelled;
        this.signatureId = signatureId;
    }
}

type Page = {
    header: string;
    tasks: (Task | Title)[];
};

function getSignatureImage() {
    const signature_list = [
        "",
        "",
        "",
        "/signatures/test.png",
        "/signatures/signature_HV_2.png",
        "/signatures/signature_HV_3.png",
        "/signatures/signature_JK_2.png",
        "/signatures/signature_JK_1.png",
        "/signatures/signature_JK_3.png",
    ];
    return signature_list[Math.floor(Math.random() * signature_list.length)];
}

async function getLarpake(client: LarpakeClient, larpakeId: number): Promise<Larpake> {
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
    const urlParams = new URLSearchParams(window.location.search);
    const larpakeId = parseInt(urlParams.get("LarpakeId") ?? "");
    const pageNum = parseInt(urlParams.get("Page") ?? "0");

    // Load data
    const client = new LarpakeClient();
    const larpake = await getLarpake(client, larpakeId);

    const unmappedTasks = (await client.getTasksByLarpakeId(larpake.id)) ?? [];
    const tasks = ToDictionary<number, LarpakeTask>(unmappedTasks, (x) => x.id);

    for (const section of larpake.sections ?? []) {
        section.tasks = tasks.get(section.id) ?? [];
    }

    const attendanceClient = new AttendanceClient();
    const attendances = await attendanceClient.get(larpake.id);

    // Render
    const renderer = new PageRenderer();
    renderer.setup(larpake.sections ?? [], attendances ?? []);
    if (!Number.isNaN(pageNum)) {
        renderer.setPage(pageNum);
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

    constructor() {
        this.header = document.getElementById("larpake-page-name") as HTMLHeadingElement;
        this.taskContainer = document.getElementById("larpake-button-container") as HTMLUListElement;
        this.indexer = document.getElementById("page-info") as HTMLParagraphElement;
        this.nextBtn = document.getElementById("next-page") as HTMLButtonElement;
        this.previousBtn = document.getElementById("prev-page") as HTMLButtonElement;
        this.pages = [];

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

    setup(sections: Section[], attendances: Attendance[]) {
        this.pages = [];

        const completions = new Map<number, Completion>();
        for (const attendance of attendances) {
            if (attendance.completed) {
                completions.set(attendance.larpakeEventId, attendance.completed);
            }
        }

        const pieces = sections.sort(SectionSortFunc).flatMap((s) => {
            const sectionTitle = getSectionText(s).title;
            const tasks = s.tasks
                .sort(TaskSortFunc)
                .map(
                    (t) =>
                        new Task(
                            t.id,
                            getTaskText(t).title,
                            t.cancelledAt != null,
                            completions.get(t.id)?.signatureId ?? null
                        )
                );

            return [new Title(sectionTitle), ...tasks];
        });

        let lastTitle: string = "";
        let span: (Title | Task)[] = [];
        for (const piece of pieces) {
            if (span.length >= PAGE_SIZE - 1) {
                this.pages.push({
                    header: `Lärpäke/${lastTitle}`,
                    tasks: span,
                });
                span = [];
            }
            span.push(piece);
        }
        if (span.length > 0){
            this.pages.push({
                header: `Lärpäke/${lastTitle}`,
                tasks: span,
            });
        }
        
    }

    render() {
        if (this.currentPage >= this.pages.length && this.pages.length !== 0) {
            window.open("statistics.html", "_self");
        }

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
            if (elem instanceof Task) {
                this.#appendTask(elem as Task);
            } else {
                this.#appendTitle(elem as Title);
            }
        }
    }

    goToPage(index: number) {
        // Use this after first render if you want to change the page
        this.setPage(index);
        this.render();
    }

    #appendTask(task: Task) {
        const element = appendTemplateElement<HTMLElement>("task-template", this.taskContainer);

        if (task.isCancelled) {
            element.classList.add("disabled");
            return;
        }

        element.querySelector<HTMLHeadingElement>("._title")!.innerText = task.title;
        const img = element.querySelector<HTMLImageElement>("._img")!;
        img.src = getSignatureImage();

        element.addEventListener("click", (_) => {
            window.location.href = `event_marking.html?EventId=${task.id}`;
        });
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
}

main();
