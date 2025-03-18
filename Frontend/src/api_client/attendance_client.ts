import { Attendance } from "../models/attendance";
import { Container } from "../models/common";
import HttpClient from "./http_client";

export default class AttendanceClient {
    client: HttpClient;

    constructor() {
        this.client = new HttpClient();
    }

    async get(larpakeId: number): Promise<Attendance[] | null> {
        const query = new URLSearchParams();
        // query.append("before", "<date>")
        // query.append("completedBefore", "<date>")
        // query.append("after", "<date>")
        // query.append("completedAfter", "<date>")
        query.append("LarpakeId", larpakeId.toString())
        query.append("LimitToSelfOnly", "false")
        // query.append("larpakeEventId", "<number>")
        // query.append("userId", "<guid>")
        // query.append("isCompleted", "<bool>")
        query.append("pageSize", "100");
        // query.append("pageOffset", "<number>")

        const response = await this.client.get("api/attendances", query);
        if (!response.ok) {
            console.warn(response);
            return null;
        }

        const attendances: Container<Attendance[]> = await response.json();
        return attendances.data;
    }
}
