using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Microsoft.Data.Sqlite;

namespace LarpakeServer.Data.Sqlite;

public class StatisticsService(
        SqliteConnectionString connectionString,
        UserDatabase userDb,
        FreshmanGroupDatabase groupDb,
        EventDatabase eventDb)
    : SqliteDbBase(connectionString, userDb, groupDb, eventDb), IStatisticsService
{
    public async Task<long?> GetFreshmanGroupPoints(int groupId)
    {
        using var connection = await GetConnection();
        return await connection.QueryFirstOrDefaultAsync<long?>($"""
            SELECT 
                COUNT(ea.{nameof(Attendance.CompletionId)}) 
            FROM FreshmanGroups fg 
                LEFT JOIN FreshmanGroupMembers fgm 
                    ON fg.{nameof(FreshmanGroup.Id)} = fgm.{nameof(FreshmanGroupMember.FreshmanGroupId)}
                LEFT JOIN EventAttendances ea
                    ON fgm.{nameof(FreshmanGroupMember.UserId)} = ea.{nameof(Attendance.UserId)}
                LEFT JOIN AttendanceCompletions ac
                    ON ea.{nameof(Attendance.CompletionId)} = ac.{nameof(AttendanceCompletion.Id)}
            WHERE 
                fg.{nameof(FreshmanGroup.Id)} = @{nameof(groupId)}    
                AND ea.{nameof(Attendance.CompletionId)} IS NOT NULL;
            """, new { groupId });
    }

    public async Task<GroupPoints[]> GetLeadingGroups(QueryOptions options)
    {
        using var connection = await GetConnection();
        var records = await connection.QueryAsync<GroupPoints>($"""
            SELECT 
                fg.{nameof(FreshmanGroup.Id)} AS {nameof(GroupPoints.GroupId)},
                COUNT(ea.{nameof(Attendance.CompletionId)}) AS {nameof(GroupPoints.Points)}
            FROM FreshmanGroups fg 
                LEFT JOIN FreshmanGroupMembers fgm 
                    ON fg.{nameof(FreshmanGroup.Id)} = fgm.{nameof(FreshmanGroupMember.FreshmanGroupId)}
                LEFT JOIN EventAttendances ea
                    ON fgm.{nameof(FreshmanGroupMember.UserId)} = ea.{nameof(Attendance.UserId)}
                LEFT JOIN AttendanceCompletions ac
                    ON ea.{nameof(Attendance.CompletionId)} = ac.{nameof(AttendanceCompletion.Id)}
            WHERE 
                ea.{nameof(Attendance.CompletionId)} IS NOT NULL;
            GROUP BY fg.{nameof(FreshmanGroup.Id)}
                ORDER BY COUNT(ea.{nameof(Attendance.CompletionId)}) DESC
                LIMIT @{nameof(QueryOptions.PageSize)}
                OFFSET @{nameof(QueryOptions.PageOffset)};
            
            """, options);
        return records.ToArray();
    }

    public async Task<long?> GetUserPoints(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        using var connection = await GetConnection();
        return await connection.QueryFirstOrDefaultAsync<long?>($"""
            SELECT 
                COUNT({nameof(Attendance.CompletionId)}) 
            FROM EventAttendances 
            WHERE {nameof(Attendance.UserId)} = @{nameof(userId)}
                AND {nameof(Attendance.CompletionId)} IS NOT NULL;
            """, new { userId });
    }

    public async Task<long[]> GetLeadingUserPoints(QueryOptions options)
    {
        using var connection = await GetConnection();
        var records = await connection.QueryAsync<long>($"""
            SELECT 
                COUNT(*) AS aCount
            FROM EventAttendances
            WHERE {nameof(Attendance.CompletionId)} IS NOT NULL
            GROUP BY {nameof(Attendance.UserId)}
                ORDER BY aCount DESC
                LIMIT @{nameof(QueryOptions.PageSize)}
                OFFSET @{nameof(QueryOptions.PageOffset)};
            """, options);
        return records.ToArray();
    }

    public async Task<UserPoints[]> GetLeadingUsers(QueryOptions options)
    {
        /* If multiple users have the same number of points,
         * This query might return more than count users.
         */

        using var connection = await GetConnection();
        var records = await connection.QueryAsync<UserPoints>($"""
            SELECT 
                {nameof(Attendance.UserId)} AS {nameof(UserPoints.UserId)},
                COUNT(*) AS {nameof(UserPoints.Points)}
            FROM EventAttendances
            WHERE {nameof(Attendance.CompletionId)} IS NOT NULL
            GROUP BY {nameof(Attendance.UserId)}
            HAVING COUNT(*) IN (
                SELECT COUNT(*) AS aCount
                    FROM EventAttendances
                WHERE {nameof(Attendance.CompletionId)} IS NOT NULL
                GROUP BY aCount
                    ORDER BY aCount DESC
                    LIMIT @{nameof(QueryOptions.PageSize)}
                    OFFSET @{nameof(QueryOptions.PageOffset)}
            );
            """, options);
        return records.ToArray();
    }

    public async Task<long> GetAveragePoints(int year)
    {
        using var connection = await GetConnection();
        return await connection.QueryFirstOrDefaultAsync<long>($"""
            SELECT 
                AVG(*)
            FROM (
                SELECT 
                    COUNT(*)
                FROM Users u
                    LEFT JOIN EventAttendances ea
                        ON u.{nameof(User.Id)} = ea.{nameof(Attendance.UserId)}
                WHERE u.{nameof(User.StartYear)} = @{nameof(year)}
                    AND ea.{nameof(Attendance.CompletionId)} IS NOT NULL
                GROUP BY {nameof(Attendance.UserId)}
            );
            """, new { year });
    }

    public async Task<long> GetTotalPoints(int year)
    {
        using var connection = await GetConnection();
        return await connection.QueryFirstOrDefaultAsync<long>($"""
            SELECT 
                COUNT(*)
            FROM Users u
                LEFT JOIN EventAttendances ea
                    ON u.{nameof(User.Id)} = ea.{nameof(Attendance.UserId)}
            WHERE u.{nameof(User.StartYear)} = @{nameof(year)}
                AND ea.{nameof(Attendance.CompletionId)} IS NOT NULL;
            """, new { year });
    }


    protected override Task InitializeAsync(SqliteConnection connection) => Task.CompletedTask;

   
}
