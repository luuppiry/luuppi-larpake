using LarpakeServer.Models;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

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
            $"SELECT larpake_id, average_points FROM GetLarpakeAverageByUser(@{nameof(userId)});", new { userId });
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
            $"SELECT larpake_id, total_points FROM GetLarpakeTotalByUser(@{nameof(userId)})", new { userId });
        return records.ToArray();
    }



    public async Task<long?> GetAveragePoints(long larpakeId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<long?>(
            $"SELECT GetLarpakeAverage(@{nameof(larpakeId)});", new { larpakeId });
    }

    public async Task<long?> GetTotalPoints(long larpakeId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<long?>(
            $"SELECT GetLarpakeTotal(@{nameof(larpakeId)});", new { larpakeId });
    }


    public async Task<LarpakeTotalPoints[]> GetUserPoints(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return [];
        }

        using var connection = GetConnection();
        var records = await connection.QueryAsync<LarpakeTotalPoints>(
            $"SELECT larpake_id, total_points FROM GetUserTotal(@{nameof(userId)});", new { userId });
        return records.ToArray();
    }

    public async Task<long?> GetGroupPoints(long groupId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<long?>(
            $"SELECT GetGroupTotal(@{nameof(groupId)});", new { groupId });
    }

    public async Task<GroupPoints[]> GetLeadingGroups(StatisticsQueryOptions options)
    {
        using var connection = GetConnection();
        var records = await connection.QueryAsync<GroupPoints>($"""
            SELECT group_id, points FROM GetLeadingGroups(
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
            SELECT user_id, points FROM GetLeadingUsers(
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
            SELECT larpake_id, group_id, total_points FROM GetGroupTotalByUser(@{nameof(userId)});
            """, new { userId });
        return records.ToArray();
    }

    public async Task<SectionPoints[]> GetOwnLarpakePoints(Guid userId, long larpakeId)
    {
        using var connection = GetConnection();
        var records = await connection.QueryAsync<SectionPoints>($"""
            SELECT 
                section_id, 
                ordering_weight_number,
                total_points,
                earned_points
            FROM GetLarpakeUserTotal(
                @{nameof(userId)},
                @{nameof(larpakeId)}
            );
            """, new { userId, larpakeId });

        return records.ToArray();
    }
}
