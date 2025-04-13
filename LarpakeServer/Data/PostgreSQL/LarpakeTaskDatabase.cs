using LarpakeServer.Data.Helpers;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.Localizations;
using LarpakeServer.Models.QueryOptions;
using Npgsql;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LarpakeServer.Data.PostgreSQL;

public class LarpakeTaskDatabase(NpgsqlConnectionString connectionString, ILogger<LarpakeTaskDatabase> logger)
    : PostgresDb(connectionString, logger), ILarpakeTaskDatabase
{
    public class LocalizationGet
    {
        public long LarpakeEventId { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public string LanguageCode { get; set; } = "";
    }


    public async Task<LarpakeTask[]> GetTasks(LarpakeTaskQueryOptions options)
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
                e.cancelled_at         
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

        query.IfNotNull(options.LarpakeTaskIds).AppendConditionLine($"""
            e.id = ANY(@{nameof(options.LarpakeTaskIds)})
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


        // Query tasks
        string parsed = query.ToString();
        using var connection = GetConnection();
        var records = await connection.QueryAsync<LarpakeTask>(parsed, options);


        // Query localizations
        long[] eventIds = records.Select(x => x.Id).ToArray();
        var localizations = await connection.QueryAsync<LocalizationGet>($"""
            SELECT 
                 title,
                 body,
                 GetLanguageCode(language_id) AS language_code,
                 larpake_event_id
            FROM larpake_event_localizations 
                WHERE larpake_event_id = ANY(@{nameof(eventIds)});
            """, new { eventIds });

        Dictionary<long, List<LarpakeTaskLocalization>> dict = localizations
            .GroupBy(x => x.LarpakeEventId)
            .ToDictionary(
                x => x.Key, x => x.Select(x => new LarpakeTaskLocalization
                {
                    Title = x.Title,
                    Body = x.Body,
                    LanguageCode = x.LanguageCode
                }).ToList()
            );

        return records.Select(x =>
        {
            if (dict.TryGetValue(x.Id, out List<LarpakeTaskLocalization>? locs))
            {
                x.TextData = locs;
            }
            return x;
        }).ToArray();
    }


    public async Task<LarpakeTask?> GetTask(long id)
    {
        using NpgsqlConnection connection = GetConnection();
        return await connection.QueryFirstOrDefaultLocalizedAsync<LarpakeTask, LarpakeTaskLocalization>($"""
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
                WHERE id = @{nameof(id)};
            """,
            new { id },
            splitOn: "title");
    }

    public async Task<Result<long>> Insert(LarpakeTask record)
    {
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            LarpakeTaskLocalization def = record.DefaultLocalization;
            long id = await connection.ExecuteScalarAsync<long>($"""
                SELECT InsertLarpakeEvent(
                    @{nameof(record.LarpakeSectionId)},
                    @{nameof(record.Points)},
                    @{nameof(record.OrderingWeightNumber)},
                    @{nameof(def.Title)},
                    @{nameof(def.Body)},
                    @{nameof(def.LanguageCode)}
                );
                """, new
            {
                record.LarpakeSectionId,
                record.Points,
                record.OrderingWeightNumber,
                def.Title,
                def.Body,
                def.LanguageCode,
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

    public async Task<Result<int>> Update(LarpakeTask record)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        int rowsAffected = await connection.ExecuteAsync($"""
            UPDATE larpake_events 
            SET
                points = @{nameof(record.Points)},
                ordering_weight_number = @{nameof(record.OrderingWeightNumber)},
                updated_at = NOW()
            WHERE id = @{nameof(record.Id)};
            """, record);

        var localizations = record.TextData
            .Select(x => new { x.Title, x.Body, record.Id, x.LanguageCode })
            .ToArray();

        rowsAffected += await connection.ExecuteAsync($"""
            INSERT INTO larpake_event_localizations (
                title,
                body, 
                larpake_event_id,
                language_id
            ) VALUES (
                @{nameof(LarpakeTaskLocalization.Title)},
                @{nameof(LarpakeTaskLocalization.Body)},
                @{nameof(record.Id)},
                GetLanguageId(@{nameof(LarpakeTaskLocalization.LanguageCode)})
            ) ON CONFLICT (larpake_event_id, language_id)
            DO UPDATE SET
                title = @{nameof(LarpakeTaskLocalization.Title)},
                body = @{nameof(LarpakeTaskLocalization.Body)};
            """, localizations);

        Debug.Assert(rowsAffected == localizations.Length + 1, "Rows affected does not match.");

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

    public async Task<int> UnsyncOrganizationEvent(long larpakeEventId, long organizationEventId)
    {
        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM event_map
            WHERE larpake_event_id = @{nameof(larpakeEventId)}
                AND organization_event_id = @{nameof(organizationEventId)};
            """, new { larpakeEventId, organizationEventId });
    }

    public async Task<int> Cancel(long id)
    {
        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE larpake_events 
            SET
                cancelled_at = NOW()
            WHERE id = @{nameof(id)}
                AND cancelled_at IS NULL;
            """, new { id });
    }

    public async Task<long[]> GetRefOrganizationEvents(long id)
    {
        using var connection = GetConnection();
        var records = await connection.QueryAsync<long>($"""
            SELECT organization_event_id
            FROM event_map
            WHERE larpake_event_id = @{nameof(id)};
            """, new { id });
        return records.ToArray();
    }


    private static async Task<long> InsertLocalizations(
        NpgsqlConnection connection, long eventId,
        IEnumerable<LarpakeTaskLocalization> localizations)
    {
        int rowsAffected = await connection.ExecuteAsync($"""
            INSERT INTO larpake_event_localizations (
                larpake_event_id,
                title,
                body,
                language_id
            )
            VALUES (
                @{nameof(eventId)},
                @{nameof(LarpakeTaskLocalization.Title)},
                @{nameof(LarpakeTaskLocalization.Body)},
                (SELECT GetLanguageId(@{nameof(LarpakeTaskLocalization.LanguageCode)}))
            );
            """, localizations.Select(x => new
        {
            eventId,
            x.Title,
            x.Body,
            x.LanguageCode
        }));
        return rowsAffected;
    }

}