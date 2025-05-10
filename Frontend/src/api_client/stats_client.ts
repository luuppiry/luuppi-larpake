import { LarpakeStatistic as LarpakeStatistics, pointsAverageStatistic, pointsTotalStatistic, groupPointsTotalStatistic, leadingGroup, leadingUser } from "../models/statistics.js";
import HttpClient from "./http_client.js";
import RequestEngine from "./request_engine.js";

export default class StatsClient extends RequestEngine {
    constructor(client: HttpClient | null = null) {
        super(client);
    }

    async getOwnLarpakeStatistics(larpakeId: number): Promise<LarpakeStatistics | null> {
        return await this.get<LarpakeStatistics>({
            url: `api/statistics/own/larpake/${larpakeId}`,
            params: null,
            failMessage: `Failed to fetch own statistics for Larpake ${larpakeId}`,
            isContainerType: false,
        });
    }

    async getOwnLarpakePointsAverage(larpakeId: number): Promise<pointsAverageStatistic | null> {
        return await this.get<pointsAverageStatistic>({
            url: `api/statistics/larpakkeet/${larpakeId}/points/average`,
            params: null,
            failMessage: `Failed to fetch own Larpake points average ${larpakeId}`,
            isContainerType: false,
        });
    }

    async getOwnLarpakeTotalPoints(larpakeId: number): Promise<pointsTotalStatistic | null> {
        return await this.get<pointsTotalStatistic>({
            url: `api/statistics/larpakkeet/${larpakeId}/points/total`,
            params: null,
            failMessage: `Failed to fetch own Larpake total points ${larpakeId}`,
            isContainerType: false,
        });
    }

    async getOwnGroupTotalPoints(groupId: number): Promise<groupPointsTotalStatistic | null> {
        return await this.get<groupPointsTotalStatistic>({
            url: `api/statistics/groups/${groupId}/points`,
            params: null,
            failMessage: `Failed to fetch own group total points ${groupId}`,
            isContainerType: false,
        });
    }

    async getLeadingUser(larpakeId: string): Promise<leadingUser | null> {
        const query = new URLSearchParams();
        query.append("LarpakeId", larpakeId);

        return await this.get<leadingUser>({
            url: `api/statistics/users/leading`,
            params: query,
            failMessage: `Failed to fetch leadind user`,
            isContainerType: false,
        });
    }

    async getLeadingGroup(larpakeId: string): Promise<leadingGroup | null> {
        const query = new URLSearchParams();
        query.append("LarpakeId", larpakeId);

        return await this.get<leadingGroup>({
            url: `api/statistics/groups/leading`,
            params: query,
            failMessage: `Failed to fetch leading group`,
            isContainerType: false,
        });
    }
}
