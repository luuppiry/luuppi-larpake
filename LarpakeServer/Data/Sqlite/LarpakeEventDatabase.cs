using LarpakeServer.Models.DatabaseModels;
using Microsoft.Data.Sqlite;

namespace LarpakeServer.Data.Sqlite;

public class LarpakeEventDatabase(
    SqliteConnectionString connectionString,
    LarpakeDatabase larpakeDb,
    OrganizationEventDatabase eventDb)
    : SqliteDbBase(connectionString, larpakeDb, eventDb), ILarpakeEventDatabase
{
    public record EventMapping(long LarpakeEventId, long OrganizationEventId);

    public async Task<LarpakeEvent[]> GetLarpakeEvents(long larpakeId)
    {
        using var connection = await GetConnection();
        var records = await connection.QueryAsync<LarpakeEvent>($"""
            SELECT * FROM LarpakeEvents
            WHERE {nameof(LarpakeEvent.LarpakeSectionId)} IN (
                SELECT {nameof(LarpakeSection.Id)} FROM LarpakeSections
                    WHERE {nameof(LarpakeSection.LarpakeId)} = {nameof(larpakeId)});
            """, new { larpakeId });
        return records.ToArray();
    }

    public async Task<LarpakeEvent[]> GetSectionEvents(long sectionId)
    {
        using var connection = await GetConnection();
        var records = await connection.QueryAsync<LarpakeEvent>($"""
            SELECT * FROM LarpakeEvents 
            WHERE {nameof(LarpakeEvent.LarpakeSectionId)} = @{nameof(sectionId)}
            """, new { sectionId });
        return records.ToArray();
    }

    public async Task<LarpakeEvent?> GetEvent(long id)
    {
        using var connection = await GetConnection();
        return await connection.QueryFirstOrDefaultAsync<LarpakeEvent>($"""
            SELECT * FROM LarpakeEvents 
            WHERE {nameof(LarpakeEvent.Id)} = @{nameof(id)};
            """, new { id });
    }

    public async Task<long> Insert(LarpakeEvent record)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteScalarAsync<long>($"""
            INSERT INTO LarpakeEvents (
                {nameof(LarpakeEvent.LarpakeSectionId)}, 
                {nameof(LarpakeEvent.Title)}, 
                {nameof(LarpakeEvent.Points)}, 
                {nameof(LarpakeEvent.Body)},
                {nameof(LarpakeEvent.CancelledAt)}) 
            VALUES (
                @{nameof(record.LarpakeSectionId)},
                @{nameof(record.Title)},
                @{nameof(record.Points)},
                @{nameof(record.Body)},
                NULL
            );
            SELECT last_insert_rowid();
            """, record);
    }

    public async Task<int> Update(LarpakeEvent record)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE LarpakeEvents 
            SET
                {nameof(LarpakeEvent.Title)} = @{nameof(record.Title)},
                {nameof(LarpakeEvent.Points)} = @{nameof(record.Points)},
                {nameof(LarpakeEvent.Body)} = @{nameof(record.Body)}
            WHERE {nameof(LarpakeEvent.Id)} = @{nameof(record.Id)};
            """, record);
    }


    public async Task<Result> SyncOrganizationEvent(long larpakeEventId, long organizationEventId)
    {
        EventMapping mapping = new(larpakeEventId, organizationEventId);

        using var connection = await GetConnection();
        try
        {
            await connection.ExecuteAsync($"""
                INSERT OR IGNORE INTO EventMap (
                    {nameof(EventMapping.LarpakeEventId)}, 
                    {nameof(EventMapping.OrganizationEventId)}) 
                VALUES (
                    @{nameof(mapping.LarpakeEventId)},
                    @{nameof(mapping.OrganizationEventId)}
                );
                """, mapping);
            return Result.Ok;
        }
        catch (SqliteException ex) when (ex.SqliteExtendedErrorCode == SqliteError.ForeignKey_e)
        {
            return Error.NotFound("One of the events does not exist.");
        }

    }

    public async Task<int> UnsyncOrganizationEvent(long larpakeEventId, long organizationEventId)
    {
        EventMapping mapping = new(larpakeEventId, organizationEventId);
        using var connection = await GetConnection();

        return await connection.ExecuteAsync($"""
            DELETE FROM EventMap 
            WHERE {nameof(EventMapping.LarpakeEventId)} = @{nameof(mapping.LarpakeEventId)}
                AND {nameof(EventMapping.OrganizationEventId)} = @{nameof(mapping.OrganizationEventId)}
            """, mapping);
    }





    protected override async Task InitializeAsync(SqliteConnection connection)
    {
        await connection.ExecuteAsync($"""
            CREATE TABLE IF NOT EXISTS LarpakeEvents (
                {nameof(LarpakeEvent.Id)} INTEGER,
                {nameof(LarpakeEvent.LarpakeSectionId)} INTEGER,
                {nameof(LarpakeEvent.Title)} TEXT NOT NULL,
                {nameof(LarpakeEvent.Body)} TEXT,
                {nameof(LarpakeEvent.CancelledAt)} DATETIME,
                {nameof(LarpakeEvent.CreatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                {nameof(LarpakeEvent.UpdatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(LarpakeEvent.Id)}),
                FOREIGN KEY ({nameof(LarpakeEvent.LarpakeSectionId)}) REFERENCES LarpakeSections({nameof(LarpakeSection.Id)})
            );

            CREATE TABLE IF NOT EXISTS EventMap (
                {nameof(EventMapping.LarpakeEventId)} INTEGER,
                {nameof(EventMapping.OrganizationEventId)} INTEGER,
                PRIMARY KEY ({nameof(EventMapping.LarpakeEventId)}, {nameof(EventMapping.OrganizationEventId)}),
                FOREIGN KEY ({nameof(EventMapping.LarpakeEventId)}) 
                    REFERENCES LarpakeEvents({nameof(LarpakeEvent.Id)}),
                FOREIGN KEY ({nameof(EventMapping.OrganizationEventId)}) 
                    REFERENCES Events({nameof(OrganizationEvent.Id)})
            """);
    }
}
