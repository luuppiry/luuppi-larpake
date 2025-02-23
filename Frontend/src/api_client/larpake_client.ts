import HttpClient from "./http_client.ts";
import { Larpake, LarpakeTask } from "../models/larpake.ts";
import { Container } from "../models/common.ts";

type Ids = {
    ids: number[];
};

export default class LarpakeClient {
    client: HttpClient;

    constructor() {
        this.client = new HttpClient();
    }

    async getOwn(): Promise<Larpake[] | null> {
        const query = new URLSearchParams();
        query.append("minimize", "false");

        const response = await this.client.get("api/larpakkeet/own", query);
        if (!response.ok) {
            console.warn(response);
            return null;
        }
        const container: Container<Larpake[]> = await response.json();
        return container.data;
    }

    async getTasks(): Promise<LarpakeTask[] | null> {
        const query = new URLSearchParams();
        // Different search params
        // query.append("userId", "<guid>");
        // query.append("groupId", "<num>");
        // query.append("sectionId", "<num>");
        // query.append("larpakeId", "<num>");
        // query.append("isCancelled", "<num>");
        // query.append("pageSize", "<num>");
        // query.append("pageOffset", "<num>");

        const response = await this.client.get("api/larpake-events", query);
        if (!response.ok) {
            console.warn(response);
            return null;
        }

        const tasks: Container<LarpakeTask[]> = await response.json();
        return tasks.data;
    }

    async getEvents(taskId: number): Promise<number[] | null> {
        if (taskId == undefined) {
            throw new Error("Task id must be defined.");
        }

        const response = await this.client.get(
            `/api/larpake-events/${taskId}/attendance-opportunities`
        );

        if (!response.ok){
            console.warn(response);
            return null;
        }

        const events: Ids = await response.json();
        return events.ids;
    }
}
