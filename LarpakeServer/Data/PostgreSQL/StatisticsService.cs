using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

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






    public Task<long?> GetFreshmanGroupPoints(long groupId)
    {
        using var connection = GetConnection();
        return connection.QueryFirstOrDefaultAsync<long?>($"""
            SELECT 
                SUM(e.points)
            FROM freshman_groups g
                LEFT JOIN freshman_group_members m
                    ON g.id = m.group_id
                LEFT JOIN event_attendances a
                    ON m.user_id = a.user_id
                LEFT JOIN larpake_events e
                    ON a.larpake_event_id = e.id
            WHERE g.id = @{nameof(groupId)}    
                AND a.completion_id IS NOT NULL;
            """, new { groupId });
    }

    public async Task<GroupPoints[]> GetLeadingGroups(StatisticsQueryOptions options)
    {
        using var connection = GetConnection();
        var records = await connection.QueryAsync<GroupPoints>($"""
            SELECT 
                g.id AS {nameof(GroupPoints.GroupId)},
                SUM(e.points) AS {nameof(GroupPoints.Points)}
            FROM freshman_groups g 
                LEFT JOIN freshman_group_members m 
                    ON g.id = m.group_id
                LEFT JOIN attendances a
                    ON m.user_id = a.user_id
                LEFT JOIN larpake_events e
                    ON a.larpake_event_id = e.id
                LEFT JOIN larpake_sections s
                    ON e.section_id = s.id
            WHERE a.completion_id IS NOT NULL
                AND s.larpake_id = @{nameof(options.LarpakeId)}
            GROUP BY g.id
                ORDER BY SUM(le.points) DESC
                LIMIT @{nameof(QueryOptions.PageSize)}
                OFFSET @{nameof(QueryOptions.PageOffset)};
            
            """, options);

        return records.ToArray();
    }

    public async Task<long[]> GetLeadingUserPoints(StatisticsQueryOptions options)
    {
        using var connection = GetConnection();
        var records = await connection.QueryAsync<long>($"""
            SELECT 
                SUM(e.points)
            FROM attendances a
                LEFT JOIN larpake_events e
                    ON a.larpake_event_id = e.id
                LEFT JOIN larpake_sections s
                    ON e.section_id = s.id
            WHERE a.completion_id IS NOT NULL
                AND s.larpake_id = @{nameof(options.LarpakeId)}
            GROUP BY a.user_id
                ORDER BY SUM(e.points) DESC
                LIMIT @{nameof(QueryOptions.PageSize)}
                OFFSET @{nameof(QueryOptions.PageOffset)};
            """, options);

        return records.ToArray();
    }

    public async Task<UserPoints[]> GetLeadingUsers(StatisticsQueryOptions options)
    {
        /* If multiple users have the same number of points,
         * This query might return more than requested users.
         */

        using var connection = GetConnection();

        var records = await connection.QueryAsync<UserPoints>($"""
            SELECT 
                a.user_id AS {nameof(UserPoints.UserId)},
                SUM(e.points) AS {nameof(UserPoints.Points)}
            FROM attendances a
                LEFT JOIN larpake_events e
                    ON a.larpake_event_id = e.id
                LEFT JOIN larpake_sections s
                    ON e.section_id = s.id
            WHERE a.completion_id IS NOT NULL
                AND s.larpake_id = @{nameof(options.LarpakeId)}
            GROUP BY a.user_id
                ORDER BY SUM(e.points) DESC
                LIMIT @{nameof(QueryOptions.PageSize)}
                OFFSET @{nameof(QueryOptions.PageOffset)};
            """, options);

        return records.ToArray();
    }






}
