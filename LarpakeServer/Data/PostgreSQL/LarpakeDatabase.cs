using LarpakeServer.Data.Helpers;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.Localizations;
using LarpakeServer.Models.QueryOptions;
using Npgsql;

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
                GetLanguageCode(sl.language_id) AS languge_code,
                ll.title,
                ll.description
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
        Dictionary<long, Larpake> larpakkeet = [];
        await connection.QueryAsync<Larpake, LarpakeLocalization, Larpake>(
            larpakeQuery.ToString(),
            (larpake, text) => Mapper.MapLocalized(larpake, text, ref larpakkeet),
            options,
            splitOn: "larpake_id");

        if (options.DoMinimize)
        {
            return larpakkeet.Values.ToArray();
        }

        Dictionary<long, LarpakeSection> sections = [];
        await connection.QueryAsync<LarpakeSection, LarpakeSectionLocalization, LarpakeSection>($"""
            SELECT 
                s.id,
                s.larpake_id,
                s.ordering_weight_number,
                s.created_at,
                s.updated_at,
                sl.larpake_section_id,
                GetLanguageCode(sl.language_id) AS languge_code
                sl.title,
            FROM larpake_sections s
                LEFT JOIN larpake_section_localizations sl
                    ON s.id = sl.larpake_section_id
            WHERE s.larpake_id IN @{nameof(larpakkeet.Keys)};
            """,
            (section, sectionLoc) => Mapper.MapLocalized(section, sectionLoc, ref sections),
            options,
            splitOn: "larpake_section_id");


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
        using var transaction = connection.BeginTransaction();

        // Insert the larpake and default localization
        LarpakeLocalization def = record.DefaultLocalization;
        long id = await connection.ExecuteScalarAsync<long>($"""
            SELECT InsertLarpake(
                @{nameof(def.Title)},
                @{nameof(record.Year)},
                @{nameof(def.Description)},
                @{nameof(def.LanguageCode)});
            """, new { def.Title, record.Year, def.Description, def.LanguageCode });

        // Insert rest of the localizations, filter default away
        await InsertLarpakeLocalizations(connection, id, 
            record.TextData.Where(x => x.LanguageCode != def.LanguageCode));

        await transaction.CommitAsync();
        return id;
    }

    public async Task<Result<int>> UpdateLarpake(Larpake record)
    {
        using var connection = GetConnection();
        using var transaction = connection.BeginTransaction();

        // Update larpake
        int rowsAffected = await connection.ExecuteAsync($"""
            UPDATE larpakkeet 
            SET 
                year = @{nameof(record.Year)},
                updated_at = NOW()
            WHERE id = @{nameof(record.Id)};
            """, record);

        // Update localiazations
        rowsAffected += await connection.ExecuteAsync($"""
            UPDATE larpake_localizations 
            SET 
                title = @{nameof(LarpakeLocalization.Title)},
                description = @{nameof(LarpakeLocalization.Description)},
            WHERE larpake_id = @{nameof(record.Id)}
                AND language_id = getlanguageid(@{nameof(LarpakeLocalization.LanguageCode)});
            """, record);

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

        Dictionary<long, LarpakeSection> sections = [];
        await connection.QueryAsync<LarpakeSection, LarpakeSectionLocalization, LarpakeSection>($"""
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
            """,
            (section, text) => Mapper.MapLocalized(section, text, ref sections),
            new
            {
                larpakeId,
                options.PageSize,
                options.PageOffset
            });

        return sections.Values.ToArray();
    }

    public async Task<LarpakeSection?> GetSection(long sectionId)
    {
        using var connection = GetConnection();

        LarpakeSection? result = null;
        await connection.QueryAsync<LarpakeSection, LarpakeSectionLocalization, LarpakeSection>($"""
            SELECT 
                s.id,
                s.larpake_id,
                s.ordering_weight_number,
                s.created_at,
                s.updated_at,
                sl.larpake_section_id,
                GetLanguageCode(sl.language_id) AS languge_code
                sl.title,
            FROM larpake_sections s
                LEFT JOIN larpake_section_localizations sl
                    ON id = larpake_section_id
            WHERE id = @{nameof(sectionId)} 
            LIMIT 1;
            """,
            (section, text) => Mapper.MapSingleLocalized(section, text, ref result),
            new { sectionId });
        return result;
    }





    public async Task<Result<long>> InsertSection(LarpakeSection section)
    {
        try
        {
            var def = section.DefaultLocalization;

            using var connection = GetConnection();
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
            });

            await InsertSectionLocalizations(connection, id, section.TextData);
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
        using var transaction = await connection.BeginTransactionAsync();

        int rowsAffected = await connection.ExecuteAsync($"""
            UPDATE larpake_sections
            SET 
                ordering_weight_number = @{nameof(record.OrderingWeightNumber)},
                updated_at = NOW()
            WHERE id = @{nameof(record.Id)};
            """, record);

        var records = record.TextData.Select(x => new { record.Id, x.LanguageCode, x.Title });
        rowsAffected += await connection.ExecuteAsync($"""
            UPDATE larpake_section_localizations
            SET 
                title = @{nameof(LarpakeSectionLocalization.Title)}
            WHERE larpake_section_id = @{nameof(record.Id)}
                AND language_id = getlanguageid(@{nameof(LarpakeSectionLocalization.LanguageCode)});
            """, record.TextData);

        await transaction.CommitAsync();
        return rowsAffected;
    }

    public Task<int> DeleteSection(long sectionId)
    {
        using var connection = GetConnection();
        return connection.ExecuteAsync($"""
            DELETE FROM larpake_sections WHERE id = @{nameof(sectionId)};
            """, new { sectionId });
    }







    private async Task InsertLarpakeLocalizations(NpgsqlConnection connection, long larpakeId, IEnumerable<LarpakeLocalization> loc)
    {
        var records = loc.Select(x => new { larpakeId, x.LanguageCode, x.Title, x.Description });
        await connection.ExecuteAsync($"""
            INSERT INTO larpake_localizations (
                larpake_id,
                language_id,
                title,
                description)
            VALUES (
                @{nameof(larpakeId)},
                (SELECT getlanguageid(@{nameof(LarpakeLocalization.LanguageCode)})),
                @{nameof(LarpakeLocalization.Title)},
                @{nameof(LarpakeLocalization.Description)}
            );
            """, records);
    }


    private async Task InsertSectionLocalizations(NpgsqlConnection connection, long sectionId, IEnumerable<LarpakeSectionLocalization> loc)
    {
        var records = loc.Select(x => new { sectionId, x.LanguageCode, x.Title });
        await connection.ExecuteAsync($"""
            INSERT INTO larpake_section_localizations (
                larpake_section_id,
                language_id,
                title)
            VALUES (
                @{nameof(sectionId)},
                (SELECT getlanguageid(@{nameof(LarpakeSectionLocalization.LanguageCode)})),
                @{nameof(LarpakeSectionLocalization.Title)}
            );
            """, records);
    }
}
