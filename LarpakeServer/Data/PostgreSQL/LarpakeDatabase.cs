using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Npgsql;

namespace LarpakeServer.Data.PostgreSQL;

public class LarpakeDatabase(NpgsqlConnectionString connectionString, ILogger<LarpakeDatabase> logger)
    : PostgresDb(connectionString, logger), ILarpakeDatabase
{
    public async Task<Larpake[]> GetLarpakkeet(QueryOptions options)
    {
        using var connection = GetConnection();
        var records = await connection.QueryAsync<Larpake>($"""
            SELECT 
                id, 
                title,
                year,
                description,
                created_at,
                updated_at
            FROM larpakkeet 
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)};
            """, options);
        return records.ToArray();
    }

    public async Task<Larpake?> GetLarpake(long larpakeId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<Larpake>($"""
            SELECT 
                id, 
                title,
                year,
                description,
                created_at,
                updated_at
            FROM larpakkeet 
            WHERE id = @{nameof(larpakeId)} 
            LIMIT 1;
            """, new { larpakeId });
    }

    public async Task<Result<long>> InsertLarpake(Larpake record)
    {
        using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<long>($"""
            INSERT INTO larpakkeet (
                title,
                year,
                description
            ) 
            VALUES (
                @{nameof(record.Title)},
                @{nameof(record.Year)},
                @{nameof(record.Description)}
            )
            RETURNING id;
            """, record);
    }

    public async Task<Result<int>> UpdateLarpake(Larpake record)
    {
        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE larpakkeet 
            SET
                title = @{nameof(record.Title)},
                year = @{nameof(record.Year)},
                description = @{nameof(record.Description)},
                updated_at = NOW()
            WHERE id = @{nameof(record.Id)};
            """, record);
    }

    public async Task<int> DeleteLarpake(long larpakeId)
    {
        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM larpakkeet WHERE id = @{nameof(larpakeId)};
            """, new { larpakeId });
    }




    public async Task<LarpakeSection[]> GetSections(QueryOptions options)
    {
        using var connection = GetConnection();
        var records = await connection.QueryAsync<LarpakeSection>($"""
            SELECT 
                id, 
                larpake_id,
                title,
                ordering_weight_number,
                created_at,
                updated_at
            FROM larpake_sections 
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)};
            """, options);

        return records.ToArray();
    }

    public async Task<LarpakeSection[]> GetLarpakeSections(long larpakeId)
    {
        using var connection = GetConnection();
        var records = await connection.QueryAsync<LarpakeSection>($"""
            SELECT 
                id, 
                larpake_id,
                title,
                ordering_weight_number,
                created_at,
                updated_at
            FROM larpake_sections 
            WHERE larpake_id = @{nameof(larpakeId)};
            """, new { larpakeId });

        return records.ToArray();
    }

    public async Task<LarpakeSection?> GetSection(long sectionId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<LarpakeSection>($"""
            SELECT 
                id, 
                larpake_id,
                title,
                ordering_weight_number,
                created_at,
                updated_at
            FROM larpake_sections 
            WHERE id = @{nameof(sectionId)} 
            LIMIT 1;
            """, new { sectionId });
    }

    public async Task<Result<long>> InsertSection(LarpakeSection section)
    {
        try
        {
            using var connection = GetConnection();
            return await connection.ExecuteScalarAsync<long>($"""
            INSERT INTO larpake_sections (
                larpake_id,
                title,
                ordering_weight_number
            ) 
            VALUES (
                @{nameof(section.LarpakeId)},
                @{nameof(section.Title)},
                @{nameof(section.SectionSequenceNumber)}
            )
            RETURNING id;
            """, section);
        }
        catch (PostgresException ex)
        {
            // TODO: Handle exception
            Logger.LogError("Failed to insert section {msg}.", ex.Message);
            throw;
        }
    }

    public async Task<Result<int>> UpdateSection(LarpakeSection record)
    {
        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE larpake_sections
            SET 
                title = @{nameof(record.Title)},
                ordering_weight_number = @{nameof(record.SectionSequenceNumber)},
                updated_at = NOW()
            WHERE id = @{nameof(record.Id)};
            """, record);
    }

    public Task<int> DeleteSection(long sectionId)
    {
        using var connection = GetConnection();
        return connection.ExecuteAsync($"""
            DELETE FROM larpake_sections WHERE id = @{nameof(sectionId)};
            """, new { sectionId });
    }
}
