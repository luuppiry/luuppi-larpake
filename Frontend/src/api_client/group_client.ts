import { GroupInfo } from "../models/user.js";
import HttpClient from "./http_client.js";

export default class GroupClient {
    client: HttpClient;
    constructor(client: HttpClient | null = null) {
        this.client = client ?? new HttpClient();
    }

    async getGroupJoinInformation(key: string): Promise<GroupInfo | 404 | null> {
        const response = await this.client.get(`api/groups/${key}/join`);
        if (response.status === 404) {
            console.warn("Group not found, link might be expired.");
            return 404;
        }
        if (!response.ok) {
            console.warn(`Failed to fetch group ${key}:`, await response.json());
            return null;
        }
        const group: GroupInfo = await response.json();
        return group;
    }

    async join(key: string): Promise<boolean | 404> {
        const response = await this.client.post(`api/groups/${key}/join`);
        if (response.status === 404) {
            return 404;
        }
        if (!response.ok) {
            console.warn(`Failed to join group '${key}'`, await response.json());
            return false;
        }
        return true;
    }
}
