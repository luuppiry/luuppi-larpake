using LarpakeServer.Data.Helpers;
using LarpakeServer.Extensions;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.Localizations;
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
                e.points,
                e.ordering_weight_number,
                e.created_at,
                e.updated_at,
                e.cancelled_at,
                loc.title,
                loc.body,
                GetLanguageCode(loc.language_id) AS language_code,
                loc.larpake_event_id
            FROM larpake_events e
                LEFT JOIN larpake_event_localizations loc
                    ON e.id = loc.larpake_event_id
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
            e.cancelled_at IS NULL
            """);

        // Only include not canceled events
        query.If(options.IsCancelled is false).AppendConditionLine($"""
            e.cancelled_at IS NOT NULL
            """);

        query.AppendLine($"""
            ORDER BY e.ordering_weight_number
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)};
            """);
        using var connection = GetConnection();
        var records = await connection.QueryLocalizedAsync<LarpakeEvent, LarpakeEventLocalization>(
            query.ToString(),
            options,
            splitOn: "title");

        return records.Values.ToArray();
    }

    public async Task<LarpakeEvent?> GetEvent(long id)
    {
        using NpgsqlConnection connection = GetConnection();
        return await connection.QueryFirstOrDefaultLocalizedAsync<LarpakeEvent, LarpakeEventLocalization>($"""
            SELECT 
                e.id, 
                e.larpake_section_id,
                e.points,
                e.ordering_weight_number,
                e.created_at,
                e.updated_at,
                e.cancelled_at,
                l.title,
                l.body
                l.language_code
                l.larpake_event_id
            FROM larpake_events e
                LEFT JOIN larpake_event_localizations loc
                    ON e.id = loc.larpake_event_id
                WHERE id = @{nameof(id)};
            """,
            new { id },
            splitOn: "title");
    }

    public async Task<Result<long>> Insert(LarpakeEvent record)
    {
        try
        {
            using var connection = GetConnection();
            using var transaction = await connection.BeginTransactionAsync();

            LarpakeEventLocalization def = record.DefaultLocalization;
            long id = await connection.ExecuteScalarAsync<long>($"""
                SELECT InsertLarpakeEvent(
                    @{nameof(record.LarpakeSectionId)},
                    @{nameof(record.Points)},
                    @{nameof(record.OrderingWeightNumber)},
                    @{nameof(def.Title)},
                    @{nameof(def.Body)},
                    @{nameof(def.LanguageCode)},
                );
                """, new 
            { 
                record.LarpakeSectionId,
                record.Points,
                record.OrderingWeightNumber,
                def.Title,
                def.Body,
                def.LanguageCode
            });

            await InsertLocalizations(connection, id, 
                record.TextData.Where(x => x.LanguageCode != def.LanguageCode));

            await transaction.CommitAsync();
            return id;
        }
        catch (PostgresException ex)
        {
            // TODO: Handle exception
            Logger.LogError("Failed to insert larpake event: {msg}", ex.Message);
            throw;
        }
    }

    public async Task<Result<int>> Update(LarpakeEvent record)
    {
        using var connection = GetConnection();
        using var transaction = await connection.BeginTransactionAsync();

        int rowsAffected = await connection.ExecuteAsync($"""
            UPDATE larpake_events 
            SET
                points = @{nameof(record.Points)},
                ordering_weight_number = @{nameof(record.OrderingWeightNumber)},
                updated_at = NOW()
            WHERE id = @{nameof(record.Id)};
            """, record);

        rowsAffected += await connection.ExecuteAsync($"""
            UPDATE larpake_event_localizations
            SET
                title = @{nameof(LarpakeEventLocalization.Title)},
                body = @{nameof(LarpakeEventLocalization.Body)}
            WHERE larpake_event_id = @{nameof(record.Id)}
                AND language_id = GetLanguageId(@{nameof(LarpakeEventLocalization.LanguageCode)});
            """, record.TextData.Select(x => new { x.Title, x.Body, record.Id, x.LanguageCode }));

        await transaction.CommitAsync();
        return rowsAffected;
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
                cancelled_at = NOW()
            WHERE id = @{nameof(id)}
                AND cancelled_at = NULL;
            """, new { id });
    }

    public async Task<Guid[]> GetRefOrganizationEvents(long id)
    {
        using var connection = GetConnection();
        var records = await connection.QueryAsync<Guid>($"""
            SELECT organization_event_id
            FROM event_map
            WHERE larpake_event_id = @{nameof(id)};
            """, new { id });
        return records.ToArray();
    }


    private static async Task InsertLocalizations(
        NpgsqlConnection connection, long eventId,
        IEnumerable<LarpakeEventLocalization> localizations)
    {
        await connection.ExecuteAsync($"""
            INSERT INTO larpake_event_localizations (
                larpake_event_id,
                title,
                body,
                language_code
            )
            VALUES (
                @{nameof(eventId)},
                @{nameof(LarpakeEventLocalization.Title)},
                @{nameof(LarpakeEventLocalization.Body)},
                @{nameof(LarpakeEventLocalization.LanguageCode)}
            );
            """, localizations.Select(x => new
            {
                eventId,
                x.Title,
                x.Body,
                x.LanguageCode
            }));
    }

}