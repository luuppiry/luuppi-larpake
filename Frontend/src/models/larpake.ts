import { getDocumentLangCode } from "../helpers.js";

export type SectionTextData = {
    title: string;
    languageCode: string;
};

export type Section = {
    id: number;
    larpakeId: number;
    orderingWeightNumber: number;
    createdAt: Date;
    updatedAt: Date;
    textData: SectionTextData[];
    /* Tasks are only here for POST requests */
    tasks: LarpakeTask[];
};

export type LarpakeTextData = {
    title: string;
    description: string;
    languageCode: "fi" | "en";
};

export type Larpake = {
    id: number;
    year: number | null;
    image_url: string | null;
    createdAt: Date;
    updatedAt: Date;
    sections: Section[] | null;
    textData: LarpakeTextData[];
};

export type LarpakeTaskTextData = {
    title: string;
    body: string;
    languageCode: string;
};

export type LarpakeTask = {
    id: number;
    larpakeSectionId: number;
    points: number;
    orderingWeightNumber: number;
    cancelledAt: Date | null;
    createdAt: Date;
    updatedAt: Date;
    textData: LarpakeTaskTextData[];
};

export function getSectionText(section: Section): SectionTextData {
    const lang = getDocumentLangCode();
    return section.textData.filter((x) => x.languageCode == lang)[0] ?? section.textData[0];
}

export function getTaskText(task: LarpakeTask) {
    const lang = getDocumentLangCode();
    return task.textData.filter((x) => x.languageCode == lang)[0] ?? task.textData[0];
}
