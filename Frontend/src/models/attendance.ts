import { LarpakeTask } from "./larpake";
import { User } from "./user";

export type Completion = {
    id: string;
    signerId: string;
    signatureId: string | null;
    completedAt: Date;
    createdAt: Date;
    updatedAt: Date;
};

export type AttendanceKey = {
    key: string;
    qrCodeKey: string;
    keyInvalidAt: Date;
};

export type Attendance = {
    userId: string;
    larpakeTaskId: number;
    completed: Completion | null;
    createdAt: Date;
    updatedAt: Date;
    key: AttendanceKey | null;
};

export type FatAttendance = {
    userId: string;
    larpakeTaskId: number;
    completed: Completion | null;
    createdAt: Date;
    updatedAt: Date;
    key: AttendanceKey | null;
    user: User;
    task: LarpakeTask;
};
