import { UI_HEADER_ID } from "../constants.js";
import { Container } from "../models/common.js";
import HttpClient from "./http_client.js";

export type GetRequest = {
    url: string;
    params: URLSearchParams | null;
    failMessage: string;
    isContainerType: boolean;
};

export type PostRequest = {
    url: string;
    body: any | null;
    failMessage: string;
    isContainerType: boolean;
};

export default class RequestEngine {
    client: HttpClient;

    constructor(client: HttpClient | null, authRequiredAction: null | (() => void) = null) {
        this.client = client ?? tryGetHttpClientFromUIHeader() ?? new HttpClient();

        // Action in case entra id requires user action
        this.client.authRequiredAction = authRequiredAction;
    }

    async get<T>(req: GetRequest): Promise<T | null> {
        const response = await this.client.get(req.url, req.params);
        if (!response.ok) {
            console.warn(req.failMessage, await response.json());
            return null;
        }
        if (req.isContainerType) {
            const records: Container<T> = await response.json();
            return records.data;
        } else {
            const records: T = await response.json();
            return records;
        }
    }

    async postStatus(req: PostRequest): Promise<boolean> {
        return (await this.post(req)) ?? false;
    }

    async post<T>(req: PostRequest): Promise<T | null> {
        const response = await this.client.post(req.url, req.body);
        if (!response.ok) {
            console.warn(req.failMessage, await response.json());
            return null;
        }
        if (req.isContainerType) {
            const container: Container<T> = await response.json();
            return container.data;
        }
        const data: T = await response.json();
        return data;
    }
}

function tryGetHttpClientFromUIHeader(): HttpClient | null {
    // Using any to solve any double import cycles
    const header = document.getElementById(UI_HEADER_ID) as any;

    if (!header || !header.getHttpClient) {
        return null;
    }

    const client = header.getHttpClient();
    if (client instanceof HttpClient) {
        return client;
    }
    return null;
}
