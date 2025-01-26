using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Microsoft.Extensions.Options;

namespace LarpakeServer.Data.PostgreSQL;

public class StatisticsService(NpgsqlConnectionString connectionString, ILogger<StatisticsService> logger)
    : PostgresDb(connectionString, logger), IStatisticsService
{
    /* For complete SQL queries, see from base folder MigrationsService/Migrations/
     */



    public async Task<LarpakeAvgPoints[]> GetAttendendLarpakeAvgPoints(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return [];
        }

        using var connection = GetConnection();
        var records = await connection.QueryAsync<LarpakeAvgPoints>(
            $"SELECT CalculateUsersLarpakeAverageUserPoints(@{nameof(userId)})", new { userId });
        return records.ToArray();
    }

    public async Task<LarpakeTotalPoints[]> GetAttendendLarpakeTotalPoints(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return [];
        }

        using var connection = GetConnection();
        var records = await connection.QueryAsync<LarpakeTotalPoints>(
            $"SELECT CalculateUsersLarpakeTotalUserPoints(@{nameof(userId)})", new { userId });
        return records.ToArray();
    }



    public async Task<long?> GetAveragePoints(long larpakeId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<long?>(
            $"SELECT CalculateLarpakeAverageUserPoints(@{nameof(larpakeId)});", new { larpakeId });
    }

    public async Task<long?> GetTotalPoints(long larpakeId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<long?>(
            $"SELECT GetLarpakeTotalPoints({nameof(larpakeId)});", new { larpakeId });
    }


    public async Task<LarpakeTotalPoints[]> GetUserPoints(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return [];
        }

        using var connection = GetConnection();
        var records = await connection.QueryAsync<LarpakeTotalPoints>(
            $"SELECT GetUserTotalPoints({nameof(userId)});", new { userId });
        return records.ToArray();
    }

    public Task<long?> GetGroupPoints(long groupId)
    {
        using var connection = GetConnection();
        return connection.QueryFirstOrDefaultAsync<long?>(
            $"SELECT GetGroupTotal(@{nameof(groupId)});", new { groupId });
    }

    public async Task<GroupPoints[]> GetLeadingGroups(StatisticsQueryOptions options)
    {
        using var connection = GetConnection();
        var records = await connection.QueryAsync<GroupPoints>($"""
            SELECT GetLeadingGroups(
                @{nameof(options.LarpakeId)},
                @{nameof(options.PageSize)},
                @{nameof(options.PageOffset)});
            """, options);
        return records.ToArray();
    }

    public async Task<UserPoints[]> GetLeadingUsers(StatisticsQueryOptions options)
    {
        using var connection = GetConnection();
        var records = await connection.QueryAsync<UserPoints>($"""
            SELECT GetLeadingUsers(
                @{nameof(options.LarpakeId)},
                @{nameof(options.PageSize)},
                @{nameof(options.PageOffset)});
            """, options);

        return records.ToArray();
    }

    public async Task<GroupTotalPoints[]> GetGroupPoints(Guid userId)
    {
        using var connection = GetConnection();
        var records = await connection.QueryAsync<GroupTotalPoints>($"""
            SELECT GetGroupTotalsByUser({nameof(userId)});
            """, new { userId });
        return records.ToArray();
    }
}
