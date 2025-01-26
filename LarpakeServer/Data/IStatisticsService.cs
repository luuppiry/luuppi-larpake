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




    Task<UserPoints[]> GetLeadingUsers(StatisticsQueryOptions options);
    Task<long[]> GetLeadingUserPoints(StatisticsQueryOptions options);
    
    Task<long?> GetFreshmanGroupPoints(long groupId);
    Task<GroupPoints[]> GetLeadingGroups(StatisticsQueryOptions options);


}