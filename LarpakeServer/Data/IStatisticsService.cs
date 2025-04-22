using LarpakeServer.Models;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;
public interface IStatisticsService
{

    Task<LarpakeAvgPoints[]> GetAttendendLarpakeAvgPoints(Guid userId);
    Task<LarpakeTotalPoints[]> GetAttendendLarpakeTotalPoints(Guid userId);
    Task<long?> GetTotalPoints(long larpakeId);
    Task<long?> GetAveragePoints(long larpakeId);
    Task<LarpakeTotalPoints[]> GetUserPoints(Guid userId);
    Task<GroupPoints[]> GetLeadingGroups(StatisticsQueryOptions options);
    Task<UserPoints[]> GetLeadingUsers(StatisticsQueryOptions options);
    Task<long?> GetGroupPoints(long groupId);
    Task<GroupTotalPoints[]> GetGroupPoints(Guid userId);
    Task<SectionPoints[]> GetOwnLarpakePoints(Guid userId, long larpakeId);
}