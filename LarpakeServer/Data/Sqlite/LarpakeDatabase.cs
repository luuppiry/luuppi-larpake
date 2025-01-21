using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Microsoft.Data.Sqlite;

namespace LarpakeServer.Data.Sqlite;

public class LarpakeDatabase(SqliteConnectionString connectionString) : SqliteDbBase(connectionString), ILarpakeDatabase
{
    public async Task<Larpake[]> GetLarpakkeet(QueryOptions options)
    {
        using var connection = await GetConnection();
        var records = await connection.QueryAsync<Larpake>($"""
            SELECT * FROM Larpakkeet 
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)};
            """, options);

        return records.ToArray();
    }

    public async Task<Larpake?> GetLarpake(long larpakeId)
    {
        using var connection = await GetConnection();
        return await connection.QueryFirstOrDefaultAsync<Larpake>($"""
            SELECT * FROM Larpakkeet WHERE {nameof(Larpake.Id)} = @{nameof(larpakeId)} LIMIT 1;
            """, new { larpakeId });
    }



    public async Task<Result<long>> InsertLarpake(Larpake record)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteScalarAsync<long>($"""
            INSERT INTO Larpakkeet (
                {nameof(Larpake.Year)}, 
                {nameof(Larpake.Title)}, 
                {nameof(Larpake.Description)}) 
            VALUES (
                @{nameof(record.Year)},
                @{nameof(record.Title)},
                @{nameof(record.Description)}
            );
            SELECT last_insert_rowid();
            """, record);
    }

    public async Task<Result<int>> UpdateLarpake(Larpake record)
    {
        using var connection = await GetConnection();

        return await connection.ExecuteAsync($"""
            UPDATE Larpakkeet 
            SET
                {nameof(Larpake.Year)} = @{nameof(record.Year)},
                {nameof(Larpake.Title)} = @{nameof(record.Title)},
                {nameof(Larpake.Description)} = @{nameof(record.Description)},
                {nameof(Larpake.UpdatedAt)} = DATETIME('now')
            WHERE {nameof(Larpake.Id)} = @{nameof(record.Id)};
            """, record);
    }

    public async Task<int> DeleteLarpake(long larpakeId)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM Larpakkeet WHERE {nameof(Larpake.Id)} = @{nameof(larpakeId)};
            """, new { larpakeId });
    }

    public async Task<LarpakeSection[]> GetSections(QueryOptions options)
    {
        using var connection = await GetConnection();
        var records = await connection.QueryAsync<LarpakeSection>($"""
            SELECT * FROM LarpakeSections 
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)};
            """, options);
        return records.ToArray();
    }

    public async Task<LarpakeSection?> GetSection(long sectionId)
    {
        using var connection = await GetConnection();
        return await connection.QueryFirstOrDefaultAsync<LarpakeSection>($"""
            SELECT * FROM Larpakkeet WHERE {nameof(LarpakeSection.Id)} = @{nameof(sectionId)} LIMIT 1;
            """, new { sectionId });
    }

    public async Task<LarpakeSection[]> GetLarpakeSections(long larpakeId)
    {
        using var connection = await GetConnection();
        var records = await connection.QueryAsync<LarpakeSection>($"""
            SELECT * FROM Larpakkeet WHERE {nameof(LarpakeSection.LarpakeId)} = @{nameof(larpakeId)};
            """, new { larpakeId });

        return records.ToArray();
    }

    public async Task<Result<long>> InsertSection(LarpakeSection record)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteScalarAsync<long>($"""
            INSERT INTO LarpakeSections (
                {nameof(LarpakeSection.LarpakeId)}, 
                {nameof(LarpakeSection.Title)}, 
                {nameof(LarpakeSection.SectionSequenceNumber)}) 
            SELECT
                @{nameof(record.LarpakeId)},
                @{nameof(record.Title)},
                MAX(SELECT {nameof(LarpakeSection.SectionSequenceNumber)}
            FROM LarpakeSections
                 WHERE {nameof(LarpakeSection.LarpakeId)} = @{nameof(record.LarpakeId)}
                 LIMIT 1)
            );
            SELECT last_insert_rowid();
            """, record);
    }

    public async Task<Result<int>> UpdateSection(LarpakeSection record)
    {
        using var connection = await GetConnection();

        return await connection.ExecuteAsync($"""
            UPDATE Larpakkeet 
            SET
                {nameof(LarpakeSection.Title)} = @{nameof(record.Title)},
                {nameof(LarpakeSection.SectionSequenceNumber)} = @{nameof(record.SectionSequenceNumber)},
                {nameof(LarpakeSection.UpdatedAt)} = DATETIME('now')
            WHERE {nameof(LarpakeSection.Id)} = @{nameof(record.Id)};
            """, record);
    }

    public async Task<int> DeleteSection(long sectionId)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM LarpakeSections WHERE {nameof(LarpakeSection.Id)} = @{nameof(sectionId)};
            """, new { sectionId });
    }




    protected override async Task InitializeAsync(SqliteConnection connection)
    {
        await connection.ExecuteAsync($"""
            CREATE TABLE IF NOT EXISTS Larpakkeet (
                {nameof(Larpake.Id)} INTEGER,
                {nameof(Larpake.Year)} INTEGER,
                {nameof(Larpake.Title)} TEXT NOT NULL,
                {nameof(Larpake.Description)} TEXT,
                {nameof(Larpake.CreatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(Larpake.UpdatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(Larpake.Id)})
            );

            CREATE TABLE IF NOT EXISTS LarpakeSections (
                {nameof(LarpakeSection.Id)} INTEGER,
                {nameof(LarpakeSection.LarpakeId)} INTEGER,
                {nameof(LarpakeSection.Title)} TEXT NOT NULL,
                {nameof(LarpakeSection.SectionSequenceNumber)} INTEGER,
                {nameof(LarpakeSection.CreatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(LarpakeSection.UpdatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(LarpakeSection.Id)}),
                FOREIGN KEY ({nameof(LarpakeSection.LarpakeId)}) REFERENCES Larpakkeet({nameof(Larpake.Id)})
            );
            """);
    }
}
