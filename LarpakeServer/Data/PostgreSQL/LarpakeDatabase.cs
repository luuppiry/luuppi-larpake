using LarpakeServer.Data.Helpers;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.Localizations;
using LarpakeServer.Models.QueryOptions;
using Npgsql;
using System.Threading;
using static System.Collections.Specialized.BitVector32;

namespace LarpakeServer.Data.PostgreSQL;

public class LarpakeDatabase(NpgsqlConnectionString connectionString, ILogger<LarpakeDatabase> logger)
    : PostgresDb(connectionString, logger), ILarpakeDatabase
{
    public async Task<Larpake[]> GetLarpakkeet(LarpakeQueryOptions options)
    {
        SelectQuery larpakeQuery = new();


        larpakeQuery.AppendLine("""
            SELECT 
                l.id, 
                l.year,
                l.created_at,
                l.updated_at,
                ll.larpake_id,
                ll.title,
                ll.description,
                GetLanguageCode(ll.language_id) AS language_code,
                ll.image_url
            FROM larpakkeet l
                LEFT JOIN larpake_localizations ll
                    ON l.id = ll.larpake_id
            """);

        // Join tables to search for user
        if (options.ContainsUser is not null)
        {
            larpakeQuery.AppendLine($"""
                LEFT JOIN freshman_groups g
                    ON l.id = g.larpake_id
                LEFT JOIN freshman_group_members m
                    ON g.id = m.group_id
                """);
            larpakeQuery.AppendConditionLine($"""
                m.user_id = @{nameof(options.ContainsUser)}
                """);
        }

        // Search for year 
        larpakeQuery.IfNotNull(options.Year).AppendConditionLine($"""
            year = @{nameof(options.Year)} 
            """);

        larpakeQuery.IfNotNull(options.LarpakeIds).AppendConditionLine($"""
            l.id = ANY(@{nameof(options.LarpakeIds)})
            """);

        // Search for title
        larpakeQuery.IfNotNull(options.Title).AppendConditionLine($"""
            title ILIKE @{nameof(options.TitleQueryValue)}
            """);

        larpakeQuery.AppendLine($"""
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)};
            """);

        using var connection = GetConnection();

        // get larpakkeet
        Dictionary<long, Larpake> larpakkeet = await connection.QueryLocalizedAsync<Larpake, LarpakeLocalization>(
            larpakeQuery.ToString(), options, splitOn: "larpake_id");

        if (options.DoMinimize)
        {
            return larpakkeet.Values.ToArray();
        }

        Dictionary<long, LarpakeSection> sections = await connection.QueryLocalizedAsync<LarpakeSection, LarpakeSectionLocalization>($"""
            SELECT 
                s.id,
                s.larpake_id,
                s.ordering_weight_number,
                s.created_at,
                s.updated_at,
                sl.larpake_section_id,
                GetLanguageCode(sl.language_id) AS language_code,
                sl.title
            FROM larpake_sections s
                LEFT JOIN larpake_section_localizations sl
                    ON s.id = sl.larpake_section_id
            WHERE s.larpake_id = ANY(@Keys);
            """,
            new { Keys = larpakkeet.Keys.ToArray() }, splitOn: "larpake_section_id");


        Dictionary<long, List<LarpakeSection>> sectionByLarpake =
            sections.Values
                .GroupBy(x => x.LarpakeId)
                .ToDictionary(x => x.Key, x => x.ToList());

        return larpakkeet.Values.Select(
            larpake =>
            {
                if (sectionByLarpake.TryGetValue(larpake.Id, out var sectionList))
                {
                    larpake.Sections = sectionList;
                }
                return larpake;
            })
            .ToArray();
    }

    public async Task<Larpake?> GetLarpake(long larpakeId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultLocalizedAsync<Larpake, LarpakeLocalization>($"""
            SELECT 
                l.id, 
                l.year,
                l.created_at,
                l.updated_at,
                ll.title,
                ll.description,
                GetLanguageCode(ll.language_id) AS language_code,
                ll.image_url
            FROM larpakkeet l
                LEFT JOIN larpake_localizations ll
                    ON l.id = ll.larpake_id
            WHERE l.id = @{nameof(larpakeId)};
            """, new { larpakeId },
            splitOn: "title");
    }

    public async Task<Result<long>> InsertLarpake(Larpake record)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        // Insert the larpake and default localization
        LarpakeLocalization def = record.DefaultLocalization;
        long id = await connection.ExecuteScalarAsync<long>($"""
            SELECT InsertLarpake(
                @{nameof(def.Title)},
                @{nameof(record.Year)},
                @{nameof(def.Description)},
                @{nameof(def.LanguageCode)},
                @{nameof(def.ImageUrl)});
            """, new { def.Title, record.Year, def.Description, def.LanguageCode, def.ImageUrl },
            transaction);

        // Insert rest of the localizations, filter default away

        var records = record.TextData
            .Where(x => x.LanguageCode != def.LanguageCode)
            .Select(x => new { id, x.LanguageCode, x.Title, x.Description, x.ImageUrl });

        await connection.ExecuteAsync($"""
            INSERT INTO larpake_localizations (
                larpake_id,
                language_id,
                title,
                description,
                image_url)
            VALUES (
                @{nameof(id)},
                (SELECT getlanguageid(@{nameof(LarpakeLocalization.LanguageCode)})),
                @{nameof(LarpakeLocalization.Title)},
                @{nameof(LarpakeLocalization.Description)},
                @{nameof(LarpakeLocalization.ImageUrl)}
            );
            """, records, transaction);



        await transaction.CommitAsync();
        return id;
    }

    public async Task<Result<int>> UpdateLarpake(Larpake record)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        // Update larpake
        int rowsAffected = await connection.ExecuteAsync($"""
            UPDATE larpakkeet 
            SET 
                year = @{nameof(record.Year)},
                updated_at = NOW()
            WHERE id = @{nameof(record.Id)};
            """, record, transaction);

        // Update localiazations
        rowsAffected += await connection.ExecuteAsync($"""
            UPDATE larpake_localizations 
            SET 
                title = @{nameof(LarpakeLocalization.Title)},
                description = @{nameof(LarpakeLocalization.Description)},
                image_url = @{nameof(LarpakeLocalization.ImageUrl)}
            WHERE larpake_id = @{nameof(record.Id)}
                AND language_id = getlanguageid(@{nameof(LarpakeLocalization.LanguageCode)});
            """, record.TextData.Select(x => new { x.Description, x.Title, x.LanguageCode, x.ImageUrl, record.Id }), 
            transaction);

        await transaction.CommitAsync();
        return rowsAffected;
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

        Dictionary<long, LarpakeSection> sections =
            await connection.QueryLocalizedAsync<LarpakeSection, LarpakeSectionLocalization>($"""
                SELECT 
                    s.id, 
                    s.larpake_id,
                    s.ordering_weight_number,
                    s.created_at,
                    s.updated_at,
                    l.title,
                    GetLanguageCode(l.language_id) AS language_code,
                    l.larpake_section_id
                FROM larpake_sections s
                    LEFT JOIN larpake_section_localizations l
                        ON s.id = l.larpake_section_id
                WHERE 
                    larpake_id = @{nameof(larpakeId)}
                ORDER BY ordering_weight_number ASC, id ASC
                LIMIT @{nameof(options.PageSize)} 
                OFFSET @{nameof(options.PageOffset)};
                """,
                new
                {
                    larpakeId,
                    options.PageSize,
                    options.PageOffset
                },
                splitOn: "title");
        return sections.Values.ToArray();
    }

    public async Task<LarpakeSection?> GetSection(long sectionId)
    {
        using var connection = GetConnection();
        LarpakeSection? result = await connection.QueryFirstOrDefaultLocalizedAsync<LarpakeSection, LarpakeSectionLocalization>($"""
            SELECT 
                s.id,
                s.larpake_id,
                s.ordering_weight_number,
                s.created_at,
                s.updated_at,
                sl.larpake_section_id,
                GetLanguageCode(sl.language_id) AS language_code,
                sl.title
            FROM larpake_sections s
                LEFT JOIN larpake_section_localizations sl
                    ON id = larpake_section_id
            WHERE id = @{nameof(sectionId)};
            """,
            new { sectionId }, splitOn: "larpake_section_id");
        return result;
    }

    public async Task<LarpakeSection?> GetSectionsByIdAndUser(long sectionId, Guid userId)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultLocalizedAsync<LarpakeSection, LarpakeSectionLocalization>($"""
            SELECT DISTINCT ON (sl.language_id)
                s.id,
                s.larpake_id,
                s.ordering_weight_number,
                s.created_at,
                s.updated_at,
                sl.larpake_section_id,
                GetLanguageCode(sl.language_id) AS language_code,
                sl.title
              FROM larpake_sections s
                LEFT JOIN larpake_section_localizations sl
                    ON id = larpake_section_id
                LEFT JOIN freshman_groups g
                    ON s.larpake_id = g.larpake_id
                LEFT JOIN freshman_group_members m
                    ON g.id = m.group_id
            WHERE s.id = @{nameof(sectionId)} 
                AND m.user_id = @{nameof(userId)};
            """, new { sectionId, userId }, splitOn: "larpake_section_id");
    }



    public async Task<Result<long>> InsertSection(LarpakeSection section)
    {
        try
        {
            var def = section.DefaultLocalization;

            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            long id = await connection.ExecuteScalarAsync<long>($"""
                SELECT InsertLarpakeSection(
                    @{nameof(section.LarpakeId)},
                    @{nameof(LarpakeSectionLocalization.Title)},
                    @{nameof(section.OrderingWeightNumber)},
                    @{nameof(LarpakeSectionLocalization.LanguageCode)});
            """, new
            {
                section.LarpakeId,
                section.OrderingWeightNumber,
                def.Title,
                def.LanguageCode
            }, transaction);


            var records = section.TextData.Where(x => x.LanguageCode != def.LanguageCode).Select(x => new { id, x.LanguageCode, x.Title });
            await connection.ExecuteAsync($"""
            INSERT INTO larpake_section_localizations (
                larpake_section_id,
                language_id,
                title
            )
            VALUES (
                @{nameof(id)},
                (SELECT getlanguageid(@{nameof(LarpakeSectionLocalization.LanguageCode)})),
                @{nameof(LarpakeSectionLocalization.Title)}
            );
            """, records);

            await transaction.CommitAsync();
            return id;

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
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        int rowsAffected = await connection.ExecuteAsync($"""
            UPDATE larpake_sections
            SET 
                ordering_weight_number = @{nameof(record.OrderingWeightNumber)},
                updated_at = NOW()
            WHERE id = @{nameof(record.Id)};
            """, record, transaction);

        var records = record.TextData.Select(x => new { record.Id, x.LanguageCode, x.Title }).ToArray();
        rowsAffected += await connection.ExecuteAsync($"""
            INSERT INTO larpake_section_localizations (
                larpake_section_id,
                language_id,
                title
            )
            VALUES (
                @{nameof(record.Id)},
                (SELECT getlanguageid(@{nameof(LarpakeSectionLocalization.LanguageCode)})),
                @{nameof(LarpakeSectionLocalization.Title)})
            ON CONFLICT (larpake_section_id, language_id)   
            DO UPDATE SET 
                title = @{nameof(LarpakeSectionLocalization.Title)}
            """, records);

        await transaction.CommitAsync();
        return rowsAffected;
    }

    public async Task<int> DeleteSection(long sectionId)
    {
        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM larpake_sections WHERE id = @{nameof(sectionId)};
            """, new { sectionId });
    }
}
