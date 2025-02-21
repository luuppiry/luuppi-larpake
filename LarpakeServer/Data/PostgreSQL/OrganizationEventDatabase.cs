using LarpakeServer.Data.Helpers;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.Localizations;
using LarpakeServer.Models.QueryOptions;
using LarpakeServer.Services.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace LarpakeServer.Data.PostgreSQL;

public class OrganizationEventDatabase : PostgresDb, IOrganizationEventDatabase
{
    readonly IntegrationOptions _integrationOptions;

    public OrganizationEventDatabase(
        NpgsqlConnectionString connectionString,
        ILogger<OrganizationEventDatabase> logger,
        IOptions<IntegrationOptions> integrationOptions) : base(connectionString, logger)
    {
        _integrationOptions = integrationOptions.Value;
    }

    public async Task<OrganizationEvent[]> Get(EventQueryOptions options)
    {
        SelectQuery query = new();

        query.AppendLine($"""
            SELECT 
                e.id, 
                e.starts_at,
                e.ends_at,
                e.created_by,
                e.created_at,
                e.updated_by,
                e.updated_at,
                e.cancelled_at,
                e.external_id,
                loc.title,
                loc.body,
                loc.location,
                loc.image_url,
                loc.website_url,
                GetLanguageCode(loc.language_id) AS language_code,
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

        OrganizationEvent[] result = records.Values.ToArray();
        
        AppendImageUrlsToAbsolute(result);
        return result;
    }



    public Task<OrganizationEvent?> Get(long id)
    {
        using var connection = GetConnection();
        return connection.QueryFirstOrDefaultLocalizedAsync<OrganizationEvent, OrganizationEventLocalization>($"""
            SELECT 
                e.id, 
                e.starts_at,
                e.ends_at,
                e.created_by,
                e.created_at,
                e.updated_by,
                e.updated_at,
                e.cancelled_at,
                e.external_id,
                loc.title,
                loc.body,
                loc.location,
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
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            OrganizationEventLocalization def = record.DefaultLocalization;
            long id = await connection.ExecuteScalarAsync<long>($"""
                SELECT InsertOrganizationEvent(
                    @{nameof(def.Title)},
                    @{nameof(def.Body)},
                    CAST(@{nameof(record.StartsAt)} AS TIMESTAMPTZ),
                    CAST(@{nameof(record.EndsAt)} AS TIMESTAMPTZ),
                    @{nameof(record.ExternalId)},
                    @{nameof(record.CreatedBy)},
                    @{nameof(def.Location)},
                    @{nameof(def.WebsiteUrl)},
                    @{nameof(def.ImageUrl)},
                    @{nameof(def.LanguageCode)});
                """, new
            {
                def.Title,
                def.Body,
                record.StartsAt,
                record.EndsAt,
                record.CreatedBy,
                record.ExternalId,
                def.Location,
                def.WebsiteUrl,
                def.ImageUrl,
                def.LanguageCode
            });

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
                    updated_at = NOW(),
                    updated_by = @{nameof(record.UpdatedBy)},
                    external_id = @{nameof(record.ExternalId)}
                WHERE 
                    id = @{nameof(record.Id)};
                """, record, transaction);

            rowsAffected += await connection.ExecuteAsync($"""
                UPDATE organization_event_localizations
                SET
                    title = @{nameof(OrganizationEventLocalization.Title)},
                    body = @{nameof(OrganizationEventLocalization.Body)},
                    website_url = @{nameof(OrganizationEventLocalization.WebsiteUrl)},
                    image_url = @{nameof(OrganizationEventLocalization.ImageUrl)},
                    location = @{nameof(OrganizationEventLocalization.Location)}
                WHERE 
                    organization_event_id = @{nameof(record.Id)}
                        AND language_id = GetLanguageId(@{nameof(OrganizationEventLocalization.LanguageCode)});
                """, record.TextData.Select(
                        x => new { record.Id, x.Title, x.Body, x.WebsiteUrl, x.ImageUrl, x.Location }),
                    transaction);

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
                location,
                language_id
            )
            VALUES (
                @{nameof(eventId)},
                @{nameof(OrganizationEventLocalization.Title)},
                @{nameof(OrganizationEventLocalization.Body)},
                @{nameof(OrganizationEventLocalization.ImageUrl)},
                @{nameof(OrganizationEventLocalization.WebsiteUrl)},
                @{nameof(OrganizationEventLocalization.Location)},
                (SELECT GetLanguageId(@{nameof(OrganizationEventLocalization.LanguageCode)}))
            );
            """, loc.Select(x => new { eventId, x.Title, x.Body, x.WebsiteUrl, x.ImageUrl, x.LanguageCode, x.Location }));
    }

    private void AppendImageUrlsToAbsolute(OrganizationEvent[] result)
    {
        foreach (OrganizationEvent record in result)
        {
            foreach (OrganizationEventLocalization localization in record.TextData)
            {
                // If image url exists and is not absolute path
                if (string.IsNullOrWhiteSpace(localization.ImageUrl) is false
                    && localization.ImageUrl.StartsWith('/'))
                {
                    localization.ImageUrl = $"{_integrationOptions.CmsApiUrlGuess}{localization.ImageUrl}";
                }
            }
        }
    }
}
