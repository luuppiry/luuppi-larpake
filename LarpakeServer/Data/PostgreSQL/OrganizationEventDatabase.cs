﻿using LarpakeServer.Data.Helpers;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.IdQueryObject;
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
            SELECT DISTINCT(id)
                id, 
                starts_at,
                ends_at,
                created_by,
                created_at,
                updated_by,
                updated_at,
                cancelled_at,
                external_id
            FROM organization_events
            """);

        query.IfTrue(options.IsFreshmanEvent).AppendLine($"""
            INNER JOIN event_map
                ON id = organization_event_id
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
            id IN (
                SELECT organization_event_id
                FROM organization_event_localizations
                WHERE title ILIKE '%' || @{nameof(options.Title)} || '%'
            )
            """);

        query.AppendLine($"""
            ORDER BY starts_at {(options.OrderDateAscending ? "ASC" : "DESC")}
            LIMIT @{nameof(options.PageSize)}
            OFFSET @{nameof(options.PageOffset)};
            """);

        string parsed = query.ToString();
        using var connection = GetConnection();
        var records = await connection.QueryAsync<OrganizationEvent>(
            parsed, options);

        OrganizationEvent[] result = records.ToArray();


        long[] ids = records.Select(x => x.Id).ToArray();
        var rawLocalizations = await connection.QueryAsync<EventIdObject>($"""
            SELECT 
                organization_event_id,
                title,
                body,
                location,
                image_url,
                website_url,
                GetLanguageCode(language_id) AS language_code
            FROM organization_event_localizations
                WHERE organization_event_id = ANY(@ids);
            """, new { ids });

        var localizations = rawLocalizations
            .GroupBy(x => x.OrganizationEventId)
            .ToDictionary(
                x => x.Key, x => x.Select(x => (OrganizationEventLocalization)x).ToList());

        foreach (var record in records)
        {
            if (localizations.TryGetValue(record.Id, out var textData))
            {
                record.TextData = textData;
            }
        }

        AppendImageUrlsToAbsolute(result);
        return result;
    }



    public async Task<OrganizationEvent?> Get(long id)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultLocalizedAsync<OrganizationEvent, OrganizationEventLocalization>($"""
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
            }, transaction);



            var localizations = record.TextData
                .Where(x => x.LanguageCode != def.LanguageCode)
                .Select(x => new { id, x.Title, x.Body, x.WebsiteUrl, x.ImageUrl, x.LanguageCode, x.Location });

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
                @{nameof(id)},
                @{nameof(OrganizationEventLocalization.Title)},
                @{nameof(OrganizationEventLocalization.Body)},
                @{nameof(OrganizationEventLocalization.ImageUrl)},
                @{nameof(OrganizationEventLocalization.WebsiteUrl)},
                @{nameof(OrganizationEventLocalization.Location)},
                (SELECT GetLanguageId(@{nameof(OrganizationEventLocalization.LanguageCode)}))
            );
            """, localizations, transaction);

            await transaction.CommitAsync();
            return id;
        }
        catch (NpgsqlException ex) when (ex.SqlState == PostgresError.NotNullConstraint)
        {
            return Error.BadRequest("Starts_at or title was null", ErrorCode.DataFieldNull);
        }
        catch (NpgsqlException ex) when (ex.SqlState == PostgresError.ForeignKeyViolation)
        {
            return Error.NotFound("Modifying user was not found", ErrorCode.IdNotFound);
        }
        catch (NpgsqlException ex) when (ex.SqlState == PostgresError.CheckConstraint)
        {
            return Error.BadRequest("External id was not unique or null", ErrorCode.IdConflict);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unhandled exception during org event insertion");
            throw;
        }
    }



    public async Task<Result<int>> Update(OrganizationEvent record)
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

    public async Task<int> SoftDelete(long eventId, Guid modifyingUser)
    {
        Logger.LogTrace("Soft deleting event {id}", eventId);

        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE organization_events 
            SET 
                cancelled_at = NOW(),
                updated_at = NOW(),
                updated_by = @{nameof(modifyingUser)}
            WHERE 
                id = @{nameof(eventId)};
            """, new { eventId, modifyingUser });
    }

    public async Task<int> HardDelete(long eventId)
    {
        Logger.LogTrace("Hard deleting event {id}", eventId);

        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM organization_events 
            WHERE id = @{nameof(eventId)};
            """, new { eventId });
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
