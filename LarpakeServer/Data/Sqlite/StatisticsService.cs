using LarpakeServer.Models.DatabaseModels;
using Microsoft.Data.Sqlite;

namespace LarpakeServer.Data.Sqlite;

public class StatisticsService(
        SqliteConnectionString connectionString,
        UserDatabase userDb,
        FreshmanGroupDatabase groupDb,
        EventDatabase eventDb)
    : SqliteDbBase(connectionString, userDb, groupDb, eventDb)
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

    public async Task<GroupPoints[]> GetLeaderGroups(int count)
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
                LIMIT @{nameof(count)};
            
            """, new { count });
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

    public async Task<long[]> GetLeadingUserPoints(int count, int offset)
    {
        using var connection = await GetConnection();
        var records = await connection.QueryAsync<long>($"""
            SELECT 
                COUNT(*) AS count
            FROM EventAttendances
            WHERE {nameof(Attendance.CompletionId)} IS NOT NULL
            GROUP BY {nameof(Attendance.UserId)}
                ORDER BY count DESC
                LIMIT @{nameof(count)}
                OFFSET @{nameof(offset)};
            """, new { count, offset });
        return records.ToArray();
    }

    public async Task<UserPoints[]> GetLeadingUsers(int count, int offset)
    {
        /* If multiple users have the same number of points,
         * This query might return more than count users.
         */

        if (count <= 0)
        {
            return [];
        }

        using var connection = await GetConnection();
        var records = await connection.QueryAsync<UserPoints>($"""
            SELECT 
                {nameof(Attendance.UserId)} AS {nameof(UserPoints.UserId)},
                COUNT(*) AS {nameof(UserPoints.Points)}
            FROM EventAttendances
            WHERE {nameof(Attendance.CompletionId)} IS NOT NULL
            GROUP BY {nameof(Attendance.UserId)}
            HAVING COUNT(*) IN (
                SELECT COUNT(*) AS count
                    FROM EventAttendances
                WHERE {nameof(Attendance.CompletionId)} IS NOT NULL
                GROUP BY count
                    ORDER BY count DESC
                    LIMIT @{nameof(count)}
                    OFFSET @{nameof(offset)}
                );
            """, new { count, offset });
        return records.ToArray();
    }

    public async Task<long> GetAverageUserPoints()
    {
        using var connection = await GetConnection();
        return await connection.QueryFirstOrDefaultAsync<long>($"""
            SELECT AVG(*)
                FROM (
                    SELECT COUNT(*)
                        FROM EventAttendances
                    WHERE {nameof(Attendance.CompletionId)} IS NOT NULL
                    GROUP BY {nameof(Attendance.UserId)}
                );
            """);

    }


    protected override Task InitializeAsync(SqliteConnection connection) => Task.CompletedTask;
}
