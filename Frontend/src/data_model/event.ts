export type EventTextData = {
    title: string;
    body: string;
    websiteUrl: string | null;
    imageUrl: string | null;
    languageCode: string;
}

export type Event = {
    id: number;
    startsAt: Date;
    endsAt: Date | null;
    createdAt: Date;
    updatedAt: Date;
    textData: EventTextData[];
    cancellationInfo: {
        deletedAt: Date;
        reason: string;
    } | null;


}