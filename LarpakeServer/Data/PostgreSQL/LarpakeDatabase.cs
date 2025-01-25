using LarpakeServer.Data.Helpers;
using LarpakeServer.Extensions;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Npgsql;

namespace LarpakeServer.Data.PostgreSQL;

public class LarpakeDatabase(NpgsqlConnectionString connectionString, ILogger<LarpakeDatabase> logger)
    : PostgresDb(connectionString, logger), ILarpakeDatabase
{
    public async Task<Larpake[]> GetLarpakkeet(LarpakeQueryOptions options)
    {
        SelectQuery query = new();

        if (options.DoMinimize)
        {
            query.AppendLine("""
            SELECT 
                l.id, 
                l.title,
                l.year,
                l.description,
                l.created_at,
                l.updated_at
            FROM larpakkeet l
            """);
        }
        else
        {
            query.AppendLine("""
            SELECT 
                l.id, 
                l.title,
                l.year,
                l.description,
                l.created_at,
                l.updated_at,
                s.id,
                s.larpake_id,
                s.title,
                s.ordering_weight_number,
                s.created_at,
                s.updated_at
            FROM larpakkeet l
            LEFT JOIN larpake_sections s
                ON l.id = s.larpake_id
            """);
        }



        // Join tables to search for user
        if (options.ContainsUser is not null)
        {
            query.AppendLine($"""
                LEFT JOIN freshman_groups g
                    ON l.id = g.larpake_id
                LEFT JOIN freshman_group_members m
                    ON g.id = m.group_id
                """);
            query.AppendConditionLine($"""
                m.user_id = @{nameof(options.ContainsUser)}
                """);
        }

        // Search for year 
        query.IfNotNull(options.Year).AppendConditionLine($"""
            year = @{nameof(options.Year)} 
            """);

        // Search for title
        query.IfNotNull(options.Title).AppendConditionLine($"""
            title ILIKE %@{nameof(options.Year)}% 
            """);

        query.AppendLine($"""
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)};
            """);

        using var connection = GetConnection();
        if (options.DoMinimize)
        {
            var minimized = await connection.QueryAsync<Larpake>(query.ToString(), options);
            return minimized.ToArray();
        }

        Dictionary<long, Larpake> unminimized = [];
        await connection.QueryAsync<Larpake, LarpakeSection, Larpake>(query.ToString(),
            (larpake, section) =>
            {
                var value = unminimized.GetOrAdd(larpake.Id, larpake)!;
                if (section is not null)
                {
                    value.Sections ??= [];
                    value.Sections.Add(section);
                }
                return value;
            },
            options,
            splitOn: "id");
        return unminimized.Values.ToArray();
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




    public async Task<LarpakeSection[]> GetSections(long larpakeId, QueryOptions options)
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
            WHERE 
                larpake_id = @{nameof(larpakeId)}
            ORDER BY ordering_weight_number ASC, id ASC
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)};
            """, new
                 {
                    larpakeId,
                    options.PageSize,
                    options.PageOffset
                 });

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
                @{nameof(section.OrderingWeightNumber)}
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
                ordering_weight_number = @{nameof(record.OrderingWeightNumber)},
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
