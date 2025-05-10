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
    data: number;
};

export type pointsTotalStatistic = {
    data: number;
};

export type groupPointsTotalStatistic = {
    data: number;
};

export type leadingUser = {
    data: [{userId: string, points: number}];
    nextPage: number;
    details: [string];
    actions: [{description: string, method: string, href: string}]
};

export type leadingGroup = {
    data: [{groupId: number, points: number}];
    nextPage: number;
    details: [string];
    actions: [{description: string, method: string, href: string}]
};
