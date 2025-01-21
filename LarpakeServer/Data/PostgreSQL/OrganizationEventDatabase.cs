using LarpakeServer.Data.Helpers;
using LarpakeServer.Models.DatabaseModels;
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
                id, 
                title,
                body,
                starts_at,
                ends_at,
                location,
                image_url,
                website_url,
                created_by,
                created_at,
                updated_by,
                updated_at,
                cancelled_at
            FROM organization_events
            """);

        query.AppendConditionLine("WHERE canceled_at IS NULL");

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
            title ILIKE %@{nameof(options.Title)}%
            """);

        query.AppendLine($"""
            ORDER BY starts_at ASC
            LIMIT @{nameof(options.PageSize)}
            OFFSET @{nameof(options.PageOffset)};
            """);

        using var connection = GetConnection();
        var records = await connection.QueryAsync<OrganizationEvent>(query.ToString(), options);
        return records.ToArray();
    }

    public Task<OrganizationEvent?> Get(long id)
    {
        using var connection = GetConnection();
        return connection.QueryFirstOrDefaultAsync<OrganizationEvent>($"""
            SELECT 
                id, 
                title,
                body,
                starts_at,
                ends_at,
                location,
                image_url,
                website_url,
                created_by,
                created_at,
                updated_by,
                updated_at,
                cancelled_at
            FROM organization_events 
            WHERE id = @id LIMIT 1;
            """, new { id });
    }

    public async Task<Result<long>> Insert(OrganizationEvent record)
    {
        try
        {
            using var connection = GetConnection();
            return await connection.QuerySingleAsync<long>($"""
                INSERT INTO organization_events (
                    title,
                    body,
                    starts_at,
                    ends_at,
                    location,
                    image_url,
                    website_url,
                    created_by,
                    updated_by,
                ) VALUES (
                    @{nameof(OrganizationEvent.Title)},
                    @{nameof(OrganizationEvent.Body)},
                    @{nameof(OrganizationEvent.StartsAt)},
                    @{nameof(OrganizationEvent.EndsAt)},
                    @{nameof(OrganizationEvent.Location)},
                    @{nameof(OrganizationEvent.ImageUrl)},
                    @{nameof(OrganizationEvent.WebsiteUrl)},
                    @{nameof(OrganizationEvent.CreatedBy)},
                    @{nameof(OrganizationEvent.CreatedBy)},
                ) RETURNING id;
                """, record);

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
            return await connection.ExecuteAsync($"""
                UPDATE organization_events 
                SET
                    title = @{nameof(OrganizationEvent.Title)},
                    body = @{nameof(OrganizationEvent.Body)},
                    starts_at = @{nameof(OrganizationEvent.StartsAt)},
                    ends_at = @{nameof(OrganizationEvent.EndsAt)},
                    location = @{nameof(OrganizationEvent.Location)},
                    image_url = @{nameof(OrganizationEvent.ImageUrl)},
                    website_url = @{nameof(OrganizationEvent.WebsiteUrl)},
                    updated_by = @{nameof(OrganizationEvent.UpdatedBy)}
                )
                WHERE id = @{nameof(OrganizationEvent.Id)};
                """, record);
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
}
