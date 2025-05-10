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

export type pointsAverageStatistic = {
    data: {averagePoints: number};
};

export type pointsTotalStatistic = {
    data: {totalPoints: number};
};

export type groupPointsTotalStatistic = {
    data: { larpakeId: number, groupId: number, totalPoints: number};
};

export type leadingUser = {
    data: { userId: string, points: number};
    nextPage: number;
    details: [string];
    actions: [{description: string, method: string, href: string}]
};

export type leadingGroup = {
    data: { groupId: number, points: number};
    nextPage: number;
    details: [string];
    actions: [{description: string, method: string, href: string}]
};
