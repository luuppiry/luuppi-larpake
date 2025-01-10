using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;
public interface IStatisticsService
{
    Task<long?> GetUserPoints(Guid userId);
    Task<UserPoints[]> GetLeadingUsers(QueryOptions options);
    Task<long[]> GetLeadingUserPoints(QueryOptions options);
    
    Task<long?> GetFreshmanGroupPoints(int groupId);
    Task<GroupPoints[]> GetLeadingGroups(QueryOptions options);
    
    Task<long> GetTotalPoints(int year);
    Task<long> GetAveragePoints(int year);
}