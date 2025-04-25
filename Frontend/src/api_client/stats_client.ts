import { LarpakeStatistic as LarpakeStatistics } from "../models/statistics.js";
import HttpClient from "./http_client.js";
import RequestEngine from "./request_engine.js";

export default class StatsClient extends RequestEngine {
    constructor(client: HttpClient | null = null, authRequiredAction: null | (() => void) = null) {
        super(client, authRequiredAction);
    }

    async getOwnLarpakeStatistics(larpakeId: number): Promise<LarpakeStatistics | null> {
        return await this.get<LarpakeStatistics>({
            url: `api/statistics/own/larpake/${larpakeId}`,
            params: null,
            failMessage: `Failed to fetch own statistics for Larpake ${larpakeId}`,
            isContainerType: false,
        });
    }
}
