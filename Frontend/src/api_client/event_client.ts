import { Container } from "../data_model/common.ts";
import HttpClient from "./http_client.ts";

export class EventClient {
    client: HttpClient;

    constructor() {
        this.client = new HttpClient();
    }

    async getAll() : Promise<Event[] | null> {
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
        const events: Container<Event[]> = await response.json();
        return events.data;
    }


    async getSpecific(id: number) : Promise<Event | null> {
        if (id == null){
            throw new Error("Event id must be defined.")
        }
        
        const response = await this.client.get(`api/org-events/${id}`, );
        if (!response.ok) {
            console.warn(response);
            return null;
        }
        const event: Event | null = await response.json();
        return event;
    }
}
