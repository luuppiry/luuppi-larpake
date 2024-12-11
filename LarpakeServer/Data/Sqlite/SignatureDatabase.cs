using LarpakeServer.Models.DatabaseModels;
using Microsoft.Data.Sqlite;

namespace LarpakeServer.Data.Sqlite;

public class SignatureDatabase(SqliteConnectionString connectionString, UserDatabase userDb)
    : SqliteDbBase(connectionString, userDb), ISignatureDatabase
{
    protected override async Task InitializeAsync(SqliteConnection connection)
    {
        await connection.ExecuteAsync($"""
            CREATE TABLE IF NOT EXISTS Signatures (
                {nameof(Signature.Id)} INTEGER,
                {nameof(Signature.UserId)} TEXT NOT NULL,
                {nameof(Signature.SignatureUrl)} TEXT NOT NULL,
                {nameof(Signature.CreatedUtc)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(Signature.Id)}),
                FOREIGN KEY ({nameof(Signature.UserId)}) REFERENCES Users({nameof(User.Id)})
            );
        """);
    }
}
