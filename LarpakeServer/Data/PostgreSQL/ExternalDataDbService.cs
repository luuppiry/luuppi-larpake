using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.External;
using LarpakeServer.Models.Localizations;
using SQLitePCL;

namespace LarpakeServer.Data.PostgreSQL;

public class ExternalDataDbService : PostgresDb, IExternalDataDbService
{
    readonly IOrganizationEventDatabase _eventDb;

    public ExternalDataDbService(
        NpgsqlConnectionString connectionString,
        IOrganizationEventDatabase eventDb,
        ILogger<ExternalDataDbService> logger) : base(connectionString, logger)
    {
        _eventDb = eventDb;
    }


    public async Task<Result<int>> SyncExternalEvents(ExternalEvent[] events)
    {
        List<OrganizationEvent> newEvents = [];

        int rowsAffected = 0;
        using var connection = GetConnection();
        try
        {
            using var transaction = await connection.BeginTransactionAsync();
            foreach (OrganizationEvent record in events.Select(x => x.ToOrganizationEvent()))
            {
                if (record.ExternalId is null)
                {
                    throw new InvalidOperationException("External event must have an external id.");
                }

                long? id = await connection.ExecuteScalarAsync<long>($"""
                    UPDATE organization_events
                    SET
                        starts_at = @{nameof(record.StartsAt)},
                        ends_at = @{nameof(record.EndsAt)},
                        updated_at = NOW(),
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
                            GetLanguageId(@{nameof(loc.LanguageCode)}),
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
                        new { id, loc.Title, loc.Body, loc.WebsiteUrl, loc.ImageUrl, loc.Location });
                }
            }

            // Insert the new events
            foreach (OrganizationEvent record in newEvents)
            {
                await _eventDb.Insert(record);
            }

            Logger.LogInformation("Synced {count} external events. ({new} where new)",
               events.Length, newEvents.Count);

            return rowsAffected;
        }
        catch (Exception ex)
        {
            Logger.LogError("Error syncing external events: {msg}", ex.Message);
            return Error.InternalServerError("Error syncing external events",
                ErrorCode.IntegrationDbWriteFailed);
        }





    }
}
