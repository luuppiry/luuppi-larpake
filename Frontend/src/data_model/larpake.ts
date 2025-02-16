import { ApiAction } from "../api_client/http_client";

export type SectionTextData = {
    title: string,
    languageCode: string
}

export type Section = {
    id: number;
    larpakeId: number;
    orderingweightNumber: number;
    createdAt: Date;
    updatedAt: Date;
    textData: SectionTextData[];
};

export type LarpakeTextData = {
    title: string;
    description: string;
    languageCode: string;
};

export type Larpake = {
    id: number;
    year: number | null;
    createdAt: Date;
    updatedAt: Date;
    sections: Section[] | null;
    textData: LarpakeTextData[];
    nextPage: number;
    details: string[] | null;
    actions: ApiAction[] | null;
};