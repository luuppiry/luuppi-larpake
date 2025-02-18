import { Attendance } from "../data_model/attendance";
import { Container } from "../data_model/common";
import HttpClient from "./http_client";

export default class AttendanceClient {
    client: HttpClient;

    constructor() {
        this.client = new HttpClient();
    }

    async getAll(): Promise<Attendance[] | null> {
        const query = new URLSearchParams();
        // query.append("before", "<date>")
        // query.append("completedBefore", "<date>")
        // query.append("after", "<date>")
        // query.append("completedAfter", "<date>")
        // query.append("larpakeEventId", "<number>")
        // query.append("userId", "<guid>")
        // query.append("isCompleted", "<bool>")
        query.append("pageSize", "100");
        // query.append("pageOffset", "<number>")

        const response = await this.client.get("/api/attendances", query);
        if (!response.ok) {
            console.warn(response);
            return null;
        }

        const attendances: Container<Attendance[]> = await response.json();
        return attendances.data;
    }
}
