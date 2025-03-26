import { Q_LARPAKE_ID, Q_LAST_PAGE, Q_OF_PAGES, Q_PAGE } from "./constants";
import { appendTemplateElement, getSearchParams, removeChildren } from "./helpers";

type Statistic = {
    title: string;
    value: number;
    max: number;
};

const data: Statistic[] = [
    {
        title: "ENSI ASKELEET",
        value: 21,
        max: 49,
    },
    {
        title: "PIENEN PIENI LUUPPILAINEN",
        value: 17,
        max: 59,
    },
    {
        title: "PII-KLUBILLA TAPAHTUU",
        value: 5,
        max: 10,
    },
    {
        title: "NORMIPÄIVÄ",
        value: 20,
        max: 23,
    },
    {
        title: "YLIOPISTOELÄMÄÄ",
        value: 8,
        max: 32,
    },
    {
        title: "VAIKUTUSVALTAA",
        value: 35,
        max: 54,
    },
    {
        title: "LIIKUNNALISTA",
        value: 13,
        max: 25,
    },
    {
        title: "KAIKENLAISTA",
        value: 24,
        max: 33,
    },
    {
        title: "TANPEREELLA",
        value: 18,
        max: 20,
    },
];

async function main() {
    const params = getSearchParams();
    const larpakeId = params.get(Q_LARPAKE_ID);

    const container = document.getElementById("statistics-container") as HTMLUListElement;

    const statistics = await fetchStatistics(parseInt(larpakeId ?? ""));
    if (statistics.length === 0) {
        return;
    }

    removeChildren(container);

    statistics.forEach((statistic) => {
        const elem = appendTemplateElement<HTMLLIElement>("statistics-value", container);
        elem.querySelector<HTMLSpanElement>("._title")!.innerText = statistic.title;
        elem.querySelector<HTMLSpanElement>("._value")!.innerText = statistic.value.toString();
        elem.querySelector<HTMLSpanElement>("._max")!.innerText = statistic.max.toString();
    });

    // Metadata
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
    console.log("Fetch Lärpäke statistics called, returning static data. Id was:", larpakeId);
    return data;
}

main();
