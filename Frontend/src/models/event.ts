export type CancellationInfo = {
    deletedAt: Date;
    reason: string;
};

export type EventLocalization = {
    title: string;
    location: string;
    body: string;
    websiteUrl: string;
    imageUrl: string;
    languageCode: "fi" | "en";
};

export type OrgEvent = {
    id: number;
    startsAt: Date;
    endsAt: Date;
    createdAt: Date;
    updatedAt: Date;
    cancellationInfo: CancellationInfo | null;
    textData: EventLocalization[];
};
