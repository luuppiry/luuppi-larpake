import { SERVER_STATUSES } from "../constants.js";
import { encodeArrayToQueryString } from "../helpers.js";
import { Attendance, AttendanceKey } from "../models/attendance.js";
import { Container, MessageResponse } from "../models/common.js";
import { Signature } from "../models/user.js";
import HttpClient from "./http_client.js";
import RequestEngine from "./request_engine.js";

export default class AttendanceClient extends RequestEngine {
    constructor(client: HttpClient | null = null) {
        super(client ?? new HttpClient());
    }

    async getAll(
        larpakeId: number | null = null,
        isSelfOnly: boolean = true,
        isCompleted: boolean | null = null,
        pageSize: number = 100,
        pageOffset: number = 0
    ): Promise<Attendance[] | null> {
        const query = new URLSearchParams();
        // query.append("before", "<date>")
        // query.append("completedBefore", "<date>")
        // query.append("after", "<date>")
        // query.append("completedAfter", "<date>")

        // query.append("larpakeEventId", "<number>")
        // query.append("userId", "<guid>")
        if (larpakeId) {
            query.append("LarpakeId", larpakeId.toString());
        }
        if (isCompleted != null) {
            query.append("isCompleted", isCompleted === true ? "true" : "false");
        }
        query.append("LimitToSelfOnly", isSelfOnly === true ? "true" : "false");
        query.append("pageSize", pageSize.toString());
        query.append("pageOffset", pageOffset.toString());

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
        if (!response.ok) {
            console.warn(await response.json());
            return [];
        }
        const signatures: Container<Signature[]> = await response.json();
        return signatures.data;
    }

    async getAttendanceByTaskId(taskId: number): Promise<AttendanceKey | number> {
        const response = await this.client.post(`api/attendances/${taskId}`);
        if (response.status === 403) {
            const error: MessageResponse | null = await response.json();
            if (error?.applicationError === SERVER_STATUSES.USER_STATUS_TUTOR) {
                return SERVER_STATUSES.USER_STATUS_TUTOR;
            }
        }
        if (!response.ok) {
            console.warn(
                `Failed to fetch attendance with task id of ${taskId}`,
                await response.json()
            );
            return response.status;
        }
        return await response.json();
    }
}
