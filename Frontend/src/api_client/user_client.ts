import { Container } from "../data_model/common.ts";
import { Group, Signature } from "../data_model/user.ts";
import HttpClient from "./http_client.ts";

export class UserClient {
    client: HttpClient;

    constructor() {
        this.client = new HttpClient();
    }

    async getOwnGroups(): Promise<Group[] | null> {
        const query = new URLSearchParams();
        query.append("doMinimize", "false");

        const response = await this.client.get("api/groups/own", query);
        if (!response.ok) {
            console.warn(response);
            return null;
        }
        const groups: Container<Group[]> = await response.json();
        return groups.data;
    }

    async getSignature(id: string): Promise<Signature | null> {
        if (id == null) {
            throw new Error("Signature id cannot be null.");
        }

        const response = await this.client.get(`api/signatures/${id}`);
        if (!response.ok) {
            console.warn(response);
            return null;
        }
        return await response.json();
    }
}
