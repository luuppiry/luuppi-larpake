using LarpakeServer.Models.DatabaseModels;
using Microsoft.Data.Sqlite;

namespace LarpakeServer.Data.Sqlite;

public class LarpakeEventDatabase(
    SqliteConnectionString connectionString, 
    LarpakeDatabase larpakeDb, 
    OrganizationEventDatabase eventDb) 
    : SqliteDbBase(connectionString, larpakeDb, eventDb)
{
    private record EventMapping(long LarpakeEventId, long OrganizationEventId);


    public async Task<LarpakeEvent[]> GetLarpakeEvents(long larpakeId)
    {
        using var connection = await GetConnection();
        var records = await connection.QueryAsync<LarpakeEvent>($"""
            SELECT * FROM Larpakkeet 
                
            WHERE {nameof(LarpakeSection.LarpakeId)} = @{nameof(larpakeId)};
            """, new { larpakeId });
        return records.ToArray();
    }

    public async Task<LarpakeEvent[]> GetSectionEvents(long sectionId)
    {
        using var connection = await GetConnection();
        var records = await connection.QueryAsync<LarpakeEvent>($"""
            SELECT * FROM LarpakeEvents 
            WHERE {nameof(LarpakeEvent.LarpakeSectionId)} = @{nameof(sectionId)};
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
