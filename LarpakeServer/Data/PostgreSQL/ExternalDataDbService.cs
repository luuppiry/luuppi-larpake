using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.External;
using LarpakeServer.Models.Localizations;
using LarpakeServer.Services.Options;
using Microsoft.Extensions.Options;

namespace LarpakeServer.Data.PostgreSQL;

public class ExternalDataDbService : PostgresDb, IExternalDataDbService
{
    readonly IOrganizationEventDatabase _eventDb;
    readonly IntegrationOptions _integrationOptions;

    public ExternalDataDbService(
        NpgsqlConnectionString connectionString,
        IOrganizationEventDatabase eventDb,
        IOptions<IntegrationOptions> integrationOptions,
        ILogger<ExternalDataDbService> logger) : base(connectionString, logger)
    {
        _eventDb = eventDb;
        _integrationOptions = integrationOptions.Value;
    }


    public async Task<Result<int>> SyncExternalEvents(ExternalEvent[] events, Guid authorId)
    {
        List<OrganizationEvent> newEvents = [];

        int rowsAffected = 0;
        using var connection = GetConnection();
        try
        {
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            foreach (OrganizationEvent record in events.Select(x => x.ToOrganizationEvent(_integrationOptions)))
            {
                if (record.ExternalId is null)
                {
                    throw new InvalidOperationException("External event must have an external id.");
                }


                record.UpdatedBy = authorId;
                long? id = await connection.ExecuteScalarAsync<long?>($"""
                    UPDATE organization_events
                    SET
                        starts_at = @{nameof(record.StartsAt)},
                        ends_at = @{nameof(record.EndsAt)},
                        updated_at = NOW(),
                        updated_by = @{nameof(record.UpdatedBy)}
                    WHERE 
                        external_id = @{nameof(record.ExternalId)}
                    RETURNING id;
                """, record, transaction);

                if (id is null)
                {
                    // Event is new and cannot be updated, will be added later
                    newEvents.Add(record);
                    continue;
                }

                rowsAffected++;

                foreach (OrganizationEventLocalization loc in record.TextData)
                {
                    // Insert or update
                    rowsAffected += await connection.ExecuteAsync($"""
                        INSERT INTO organization_event_localizations(
                            organization_event_id, 
                            language_id, 
                            title, 
                            body, 
                            website_url, 
                            image_url, 
                            location
                        ) VALUES (
                            @{nameof(id)},
                            (SELECT GetLanguageId(@{nameof(loc.LanguageCode)})),
                            @{nameof(loc.Title)},
                            @{nameof(loc.Body)},
                            @{nameof(loc.WebsiteUrl)},
                            @{nameof(loc.ImageUrl)},
                            @{nameof(loc.Location)}
                        )
                        ON CONFLICT (organization_event_id, language_id) 
                            DO UPDATE SET
                                title = @{nameof(loc.Title)},
                                body = @{nameof(loc.Body)},
                                website_url = @{nameof(loc.WebsiteUrl)},
                                image_url = @{nameof(loc.ImageUrl)},
                                location = @{nameof(loc.Location)};
                        """,
                        new { id, loc.Title, loc.Body, loc.WebsiteUrl, loc.ImageUrl, loc.Location, loc.LanguageCode },
                        transaction);
                }
            }

            await transaction.CommitAsync();

            // Insert the new events
            foreach (OrganizationEvent record in newEvents)
            {
                record.CreatedBy = authorId;
                record.UpdatedBy = authorId;

                await _eventDb.Insert(record);
            }

            Logger.LogTrace("Synced {count} external events. ({new} where new)",
               events.Length, newEvents.Count);

            return rowsAffected + newEvents.Count;
        }
        catch (Exception ex)
        {
            Logger.LogError("Error syncing external events: {msg}.", ex.Message);
            return Error.InternalServerError("Error syncing external events",
                ErrorCode.IntegrationDbWriteFailed);
        }
    }
}
