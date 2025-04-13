import { Container } from "../models/common.js";
import { OrgEvent } from "../models/event.js";
import HttpClient from "./http_client.js";
import RequestEngine from "./request_engine.js";

export class EventClient extends RequestEngine {
    constructor(client: HttpClient | null = null) {
        super(client );
    }

    async nextComing(): Promise<OrgEvent[] | null> {
        const today = new Date();
        const date = new Date(today.getFullYear(), today.getMonth(), today.getDate());

        const query = new URLSearchParams();
        query.append("doMinimize", "false");
        query.append("pageSize", "8");
        query.append("after", date.toISOString());

        const response = await this.client.get("api/org-events", query);
        if (!response.ok) {
            console.warn(await response.json());
            return null;
        }
        const events: Container<OrgEvent[]> = await response.json();
        return events.data;
    }

    async getAll(): Promise<OrgEvent[] | null> {
        const query = new URLSearchParams();
        query.append("doMinimize", "false");
        query.append("pageSize", "100");

        // Different search params
        // query.append("before", "<date>");
        // query.append("after", "<date>");
        // query.append("title", "<string>");
        // query.append("pageSize", "<num>");
        // query.append("pageOffset", "<num>");

        const response = await this.client.get("api/org-events", query);
        if (!response.ok) {
            console.warn(response);
            return null;
        }
        const events: Container<OrgEvent[]> = await response.json();
        return events.data;
    }

    async getSpecific(id: number): Promise<OrgEvent | null> {
        if (id == null) {
            throw new Error("Event id must be defined.");
        }

        const response = await this.client.get(`api/org-events/${id}`);
        if (!response.ok) {
            console.warn(response);
            return null;
        }
        const event: OrgEvent | null = await response.json();
        return event;
    }
}
