using Npgsql;

namespace LarpakeServer.Data.PostgreSQL;

public class PostgresDb(NpgsqlConnectionString connectionString)
{
    protected NpgsqlConnectionString ConnectionString { get; } = connectionString;

    protected NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(ConnectionString.Value);
    }
}
