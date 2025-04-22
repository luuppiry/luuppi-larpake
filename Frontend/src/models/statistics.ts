export type LarpakeStatistic = {
    larpakeId: number;
    userId: string;
    data: SectionStatistic[];
};

export type SectionStatistic = {
    sectionId: number;
    orderingWeightNumber: number;
    totalPoints: number;
    earnedPoints: number;
};
