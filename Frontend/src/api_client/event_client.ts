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
        // Take more than needed because of pageing issues
        query.append("pageSize", "20");     
        query.append("after", date.toISOString());
        query.append("OrderDateAscending", "true");


        const res = await this.get<OrgEvent[]>({
            url: "api/org-events",
            params: query,
            failMessage: "Failed to fetch coming organization events",
            isContainerType: true
        });
        return res?.slice(0, 5) ?? null;
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

        return await this.get<OrgEvent[]>({
            url: "api/org-events",
            params: query,
            failMessage: "Failed to fetch all organization events",
            isContainerType: true
        })
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

    async pullExternal(): Promise<void> {
        const response = await this.client.get("api/org-events/pull-external-server-events");
        if (!response.ok){
            console.warn("Failed to pull external events:", await response.json());
            throw new Error("Method not implemented.");
        }
    }
}
