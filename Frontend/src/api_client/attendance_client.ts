import { encodeArrayToQueryString } from "../helpers";
import { Attendance } from "../models/attendance";
import { Container } from "../models/common";
import { Signature } from "../models/user";
import HttpClient from "./http_client";

export default class AttendanceClient {
    client: HttpClient;

    constructor(client: HttpClient | null = null) {
        this.client = client ?? new HttpClient();
    }

    async get(larpakeId: number): Promise<Attendance[] | null> {
        const query = new URLSearchParams();
        // query.append("before", "<date>")
        // query.append("completedBefore", "<date>")
        // query.append("after", "<date>")
        // query.append("completedAfter", "<date>")
        query.append("LarpakeId", larpakeId.toString());
        query.append("LimitToSelfOnly", "false");
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

    async getSignature(signatureId: string): Promise<Signature | null> {
        const respose = await this.client.get(`api/signatures/${signatureId}`);
        if (respose.status === 404) {
            return null;
        }
        if (!respose.ok) {
            const error = await respose.json();
            throw new Error(`Request to signature endpoint failed: ${error}`);
        }
        return await respose.json();
    }

    async getSignatures(signatureIds: string[]): Promise<Signature[]> {
        const ids = encodeArrayToQueryString("SignatureIds", signatureIds);
        const response = await this.client.get(`api/signatures?${ids}`);
        if (!response.ok){
            console.warn(await response.json())
            return [];
        }
        const signatures: Container<Signature[]> = await response.json();
        return signatures.data;
    }
}
