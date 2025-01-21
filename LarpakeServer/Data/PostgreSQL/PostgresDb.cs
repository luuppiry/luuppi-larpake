using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace LarpakeServer.Data.PostgreSQL;

public class PostgresDb(NpgsqlConnectionString connectionString, ILogger<PostgresDb>? logger = null)
{
    protected NpgsqlConnectionString ConnectionString { get; } = connectionString;
    protected virtual ILogger<PostgresDb> Logger { get; } = logger ?? NullLogger<PostgresDb>.Instance;

    protected NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(ConnectionString.Value);
    }
}
