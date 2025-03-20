export type Completion = {
    id: string;
    signerId: string;
    signatureId: string | null;
    completedAt: Date;
    createdAt: Date;
    updatedAt: Date;
}

export type AttendanceKey = {
    qrCodeKey: string;
    keyInvalidAt: Date;
}

export type Attendance = {
    userId: string;
    larpakeEventId: number;
    completed: Completion | null;
    createdAt: Date;
    updatedAt: Date;
    key: AttendanceKey | null;
}