using LarpakeServer.Data.Helpers;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.Localizations;
using LarpakeServer.Models.QueryOptions;
using Npgsql;

namespace LarpakeServer.Data.PostgreSQL;

public class OrganizationEventDatabase(NpgsqlConnectionString connectionString, ILogger<OrganizationEventDatabase> logger)
    : PostgresDb(connectionString, logger), IOrganizationEventDatabase
{
    public async Task<OrganizationEvent[]> Get(EventQueryOptions options)
    {
        SelectQuery query = new();

        query.AppendLine($"""
            SELECT 
                e.id, 
                e.starts_at,
                e.ends_at,
                e.location,
                e.created_by,
                e.created_at,
                e.updated_by,
                e.updated_at,
                e.cancelled_at
                loc.title,
                loc.body,
                loc.image_url,
                loc.website_url,
                loc.language_code,
                loc.organization_event_id
            FROM organization_events e
                LEFT JOIN organization_event_localizations loc
                    ON e.id = loc.organization_event_id
            """);

        query.AppendConditionLine("cancelled_at IS NULL");

        // If event after given time
        query.IfNotNull(options.After).AppendConditionLine($"""
            starts_at >= @{nameof(options.After)}
            """);

        // If event before given time
        query.IfNotNull(options.Before).AppendConditionLine($"""
            starts_at <= @{nameof(options.Before)}
            """);

        // If event title like string
        query.IfNotNull(options.Title).AppendConditionLine($"""
            e.id IN (
                SELECT organization_event_id
                FROM organization_event_localizations
                WHERE title ILIKE '%' || @{nameof(options.Title)} || '%'
            )
            """);

        query.AppendLine($"""
            ORDER BY starts_at ASC
            LIMIT @{nameof(options.PageSize)}
            OFFSET @{nameof(options.PageOffset)};
            """);

        using var connection = GetConnection();
        var records = await connection.QueryLocalizedAsync<OrganizationEvent, OrganizationEventLocalization>(
            query.ToString(), options, splitOn: "title");
        return records.Values.ToArray();
    }

    public Task<OrganizationEvent?> Get(long id)
    {
        using var connection = GetConnection();
        return connection.QueryFirstOrDefaultLocalizedAsync<OrganizationEvent, OrganizationEventLocalization>($"""
            SELECT 
                e.id, 
                e.starts_at,
                e.ends_at,
                e.location,
                e.created_by,
                e.created_at,
                e.updated_by,
                e.updated_at,
                e.cancelled_at,
                loc.title,
                loc.body,
                loc.image_url,
                loc.website_url,
                loc.language_code,
                loc.organization_event_id
            FROM organization_events e
                LEFT JOIN organization_event_localizations loc
                    ON e.id = loc.organization_event_id
            WHERE id = @id;
            """, new { id });
    }

    public async Task<Result<long>> Insert(OrganizationEvent record)
    {
        try
        {
            using var connection = GetConnection();
            using var transaction = await connection.BeginTransactionAsync();

            OrganizationEventLocalization def = record.DefaultLocalization;
            long id = await connection.QuerySingleAsync<long>($"""
                SELECT InsertOrganizationEvent(
                    @{nameof(def.Title)},
                    @{nameof(def.Body)},
                    @{nameof(record.StartsAt)},
                    @{nameof(record.EndsAt)},
                    @{nameof(record.Location)},
                    @{nameof(record.CreatedBy)},
                    @{nameof(def.WebsiteUrl)},
                    @{nameof(def.ImageUrl)},
                    @{nameof(def.LanguageCode)});
                """, record);

            await InsertLocalizations(connection, id,
                record.TextData.Where(x => x.LanguageCode != def.LanguageCode));

            await transaction.CommitAsync();
            return id;
        }
        catch (Exception e)
        {
            // TODO: Handle exception
            Logger.LogError("Error inserting org event: {msg}", e.Message);
            throw;
        }
    }



    public async Task<Result<int>> Update(OrganizationEvent record)
    {
        try
        {
            using var connection = GetConnection();
            using var transaction = await connection.BeginTransactionAsync();

            int rowsAffected = await connection.ExecuteAsync($"""
                UPDATE organization_events 
                SET 
                    starts_at = @{nameof(record.StartsAt)},
                    ends_at = @{nameof(record.EndsAt)},
                    location = @{nameof(record.Location)},
                    updated_at = NOW(),
                    updated_by = @{nameof(record.UpdatedBy)}
                WHERE 
                    id = @{nameof(record.Id)};
                """, record);

            rowsAffected += await connection.ExecuteAsync($"""
                UPDATE organization_event_localizations
                SET
                    title = @{nameof(OrganizationEventLocalization.Title)},
                    body = @{nameof(OrganizationEventLocalization.Body)},
                    website_url = @{nameof(OrganizationEventLocalization.WebsiteUrl)},
                    image_url = @{nameof(OrganizationEventLocalization.ImageUrl)}
                WHERE 
                    organization_event_id = @{nameof(record.Id)}
                        AND language_id = GetLanguageId(@{nameof(OrganizationEventLocalization.LanguageCode)});
                """, record.TextData.Select(
                        x => new { record.Id, x.Title, x.Body, x.WebsiteUrl, x.ImageUrl }));

            await transaction.CommitAsync();
            return rowsAffected;
        }
        catch (PostgresException ex)
        {
            // TODO: Handle exception
            Logger.LogError("Error updating org event: {msg}", ex.Message);
            throw;
        }
    }

    public Task<int> SoftDelete(long eventId, Guid modifyingUser)
    {
        Logger.LogInformation("Soft deleting event {id}", eventId);

        using var connection = GetConnection();
        return connection.ExecuteAsync($"""
            UPDATE organization_events 
            SET 
                cancelled_at = NOW(),
                updated_at = NOW(),
                updated_by = @{nameof(modifyingUser)}
            WHERE 
                id = @{nameof(eventId)};
            """, new { eventId, modifyingUser });
    }

    public Task<int> HardDelete(long eventId)
    {
        Logger.LogInformation("Hard deleting event {id}", eventId);

        using var connection = GetConnection();
        return connection.ExecuteAsync($"""
            DELETE FROM organization_events 
            WHERE id = @{nameof(eventId)};
            """, new { eventId });
    }




    private static async Task InsertLocalizations(NpgsqlConnection connection, long eventId, IEnumerable<OrganizationEventLocalization> loc)
    {
        await connection.ExecuteAsync($"""
            INSERT INTO organization_event_localizations (
                organization_event_id,
                title,
                body,
                image_url,
                website_url,
                language_code
            )
            VALUES (
                @{nameof(eventId)}
                @{nameof(OrganizationEventLocalization.Title)},
                @{nameof(OrganizationEventLocalization.Body)},
                @{nameof(OrganizationEventLocalization.ImageUrl)},
                @{nameof(OrganizationEventLocalization.WebsiteUrl)},
                (SELECT GetLanguageId(@{nameof(OrganizationEventLocalization.LanguageCode)}))
            );
            """, loc.Select(x => new { eventId, x.Title, x.Body, x.WebsiteUrl, x.ImageUrl, x.LanguageCode }));
    }
}
