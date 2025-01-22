using LarpakeServer.Data.Helpers;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Npgsql;

namespace LarpakeServer.Data.PostgreSQL;

public class LarpakeEventDatabase(NpgsqlConnectionString connectionString, ILogger<LarpakeEventDatabase> logger)
    : PostgresDb(connectionString, logger), ILarpakeEventDatabase
{
    public async Task<LarpakeEvent[]> GetEvents(LarpakeEventQueryOptions options)
    {
        SelectQuery query = new();

        query.AppendLine($"""
             SELECT 
                e.id, 
                e.larpake_section_id,
                e.title,
                e.body,
                e.points,
                e.ordering_weight_number,
                e.created_at,
                e.updated_at,
                e.canceled_at
            FROM larpake_events e
            """);

        bool requireSections = options.LarpakeId is not null || options.UserId is not null;

        // Join larpake sections
        query.If(requireSections).AppendLine($"""
            LEFT JOIN larpake_sections s
                ON e.larpake_section_id = s.id
            """);

        // Join larpakkeet
        query.IfNotNull(options.UserId).AppendLine($"""
            LEFT JOIN larpakkeet l
                ON s.larpake_id = l.id
            """);
        
        // Filter larpakkeet, which user participates in 
        query.IfNotNull(options.UserId).AppendConditionLine($"""
            l.id IN (
                SELECT g.larpake_id
                FROM freshman_group_members m
                    LEFT JOIN freshman_groups g
                        ON m.group_id = g.id
            )
            """);


        // Filter by larpake id
        query.IfNotNull(options.LarpakeId).AppendConditionLine($"""
            s.larpake_id = @{nameof(options.LarpakeId)}
            """);

        // Filter by section id
        query.IfNotNull(options.SectionId).AppendLine($"""
            e.larpake_section_id = @{nameof(options.SectionId)}
            """);

        // Only include canceled events
        query.If(options.IsCancelled is true).AppendConditionLine($"""
            e.canceled_at IS NULL
            """);

        // Only include not canceled events
        query.If(options.IsCancelled is false).AppendConditionLine($"""
            e.canceled_at IS NOT NULL
            """);

        query.AppendLine($"""
            ORDER BY e.ordering_weight_number, e.title ASC
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)};
            """);


        using var connection = GetConnection();
        var records = await connection.QueryAsync<LarpakeEvent>(query.ToString(), options);
        return records.ToArray();
    }

    public Task<LarpakeEvent?> GetEvent(long id)
    {
        using var connection = GetConnection();
        return connection.QueryFirstOrDefaultAsync<LarpakeEvent>($"""
            SELECT 
                id, 
                larpake_section_id,
                title,
                body,
                points,
                ordering_weight_number,
                created_at,
                updated_at,
                canceled_at
            FROM larpake_events 
            WHERE id = @{nameof(id)} 
            LIMIT 1;
            """, new { id });
    }

    public Task<long> Insert(LarpakeEvent record)
    {
        try
        {
            using var connection = GetConnection();
            return connection.ExecuteScalarAsync<long>($"""
            INSERT INTO larpake_events (
                larpake_section_id, 
                title, 
                body,
                points, 
                ordering_weight_number,
                canceled_at
                ) 
            VALUES (
                @{nameof(record.LarpakeSectionId)},
                @{nameof(record.Title)},
                @{nameof(record.Body)},
                @{nameof(record.Points)},
                @{nameof(record.OrderingWeightNumber)},
                NULL
            )
            RETURNING id;
            """, record);
        }
        catch (PostgresException ex)
        {
            // TODO: Handle exception
            Logger.LogError("Failed to insert larpake event: {msg}", ex.Message);
            throw;
        }
    }

    public Task<int> Update(LarpakeEvent record)
    {
        using var connection = GetConnection();
        return connection.ExecuteAsync($"""
            UPDATE larpake_events 
            SET
                title = @{nameof(record.Title)},
                body = @{nameof(record.Body)}
                points = @{nameof(record.Points)},
                ordering_weight_number = @{nameof(record.OrderingWeightNumber)},
                updated_at = NOW()
            WHERE id = @{nameof(record.Id)};
            """, record);
    }

    public async Task<Result> SyncOrganizationEvent(long larpakeEventId, long organizationEventId)
    {
        using var connection = GetConnection();
        try
        {
            await connection.ExecuteAsync($"""
                INSERT INTO event_map (
                    larpake_event_id, 
                    organization_event_id
                )
                VALUES (
                    @{nameof(larpakeEventId)},
                    @{nameof(organizationEventId)}
                )
                ON CONFLICT DO NOTHING;
                """, new { larpakeEventId, organizationEventId });

            return Result.Ok;
        }
        catch (PostgresException ex)
        {
            // TODO: Handle exception
            Logger.LogError("Failed to events: {msg}", ex.Message);
            throw;
        }
    }

    public Task<int> UnsyncOrganizationEvent(long larpakeEventId, long organizationEventId)
    {
        using var connection = GetConnection();
        return connection.ExecuteAsync($"""
            DELETE FROM event_map
            WHERE larpake_event_id = @{nameof(larpakeEventId)}
                AND organization_event_id = @{nameof(organizationEventId)};
            """, new { larpakeEventId, organizationEventId });
    }

    public Task<int> Cancel(long id)
    {
        using var connection = GetConnection();
        return connection.ExecuteAsync($"""
            UPDATE larpake_events 
            SET
                canceled_at = NOW()
            WHERE id = @{nameof(id)};
            """, new { id });
    }
}
