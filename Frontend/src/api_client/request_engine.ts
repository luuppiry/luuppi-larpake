import { Container } from "../models/common";
import HttpClient from "./http_client";

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
    constructor(client: HttpClient) {
        this.client = client;
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
