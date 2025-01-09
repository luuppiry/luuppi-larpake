using LarpakeServer.Helpers.Generic;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;
using Microsoft.Data.Sqlite;

namespace LarpakeServer.Data.Sqlite;

public class SignatureDatabase(SqliteConnectionString connectionString, UserDatabase userDb)
    : SqliteDbBase(connectionString, userDb), ISignatureDatabase
{
    public async Task<Signature[]> Get(SignatureQueryOptions options)
    {
        StringBuilder query = new();
        query.AppendLine($"""
            SELECT * FROM Signatures
            """);
        if (options.UserId is not null)
        {
            query.AppendLine($"""
                WHERE {nameof(Signature.UserId)} = @{nameof(options.UserId)}
                """);
        }
        query.AppendLine($"""
            ORDER BY {nameof(Signature.UserId)} ASC, {nameof(Signature.CreatedAt)} DESC
            LIMIT @{nameof(options.PageSize)} 
            OFFSET @{nameof(options.PageOffset)}
            """);

        using var connection = await GetConnection();
        var records = await connection.QueryAsync<Signature>(query.ToString(), options);
        return records.ToArray();
    }

    public async Task<Signature?> Get(Guid id)
    {
        using var connection = await GetConnection();
        return await connection.QueryFirstOrDefaultAsync<Signature>($"""
            SELECT * FROM Signatures WHERE {nameof(Signature.Id)} = @{nameof(id)} LIMIT 1;
        """, new { id });
    }

    public async Task<Result<Guid>> Insert(Signature record)
    {
        record.Id = Guid.CreateVersion7();

        using var connection = await GetConnection();
        await connection.ExecuteAsync($"""
            INSERT INTO Signatures (
                {nameof(Signature.Id)},
                {nameof(Signature.UserId)},
                {nameof(Signature.PathDataJson)},
                {nameof(Signature.Height)},
                {nameof(Signature.Width)},
                {nameof(Signature.LineWidth)},
                {nameof(Signature.StrokeStyle)},
                {nameof(Signature.LineCap)}
            ) VALUES (
                @{nameof(record.Id)},
                @{nameof(record.UserId)},
                @{nameof(record.PathDataJson)},
                @{nameof(record.Height)},
                @{nameof(record.Width)},
                @{nameof(record.LineWidth)},
                @{nameof(record.StrokeStyle)},
                @{nameof(record.LineCap)}
            );
        """, record);

        return record.Id;
    }

    public async Task<Result<int>> Delete(Guid id)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM Signatures WHERE {nameof(Signature.Id)} = @{nameof(id)};
        """, new { id });
    }

    protected override async Task InitializeAsync(SqliteConnection connection)
    {
        await connection.ExecuteAsync($"""
            CREATE TABLE IF NOT EXISTS Signatures (
                {nameof(Signature.Id)} TEXT,
                {nameof(Signature.UserId)} TEXT NOT NULL,
                {nameof(Signature.PathDataJson)} TEXT NOT NULL,
                {nameof(Signature.Height)} INTEGER NOT NULL,
                {nameof(Signature.Width)} INTEGER NOT NULL,
                {nameof(Signature.LineWidth)} INTEGER DEFAULT 2,
                {nameof(Signature.StrokeStyle)} TEXT,
                {nameof(Signature.LineCap)} TEXT,
                {nameof(Signature.CreatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(Signature.Id)}),
                FOREIGN KEY ({nameof(Signature.UserId)}) REFERENCES Users({nameof(User.Id)})
            );
        """);
    }


}
