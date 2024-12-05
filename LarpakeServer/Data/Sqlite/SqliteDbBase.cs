using LarpakeServer.Helpers;
using Microsoft.Data.Sqlite;

namespace LarpakeServer.Data.Sqlite;

public abstract class SqliteDbBase
{
    protected SqliteConnectionString _connectionString;
    protected bool _isInitialized = false;


    protected SqliteDbBase(SqliteConnectionString connectionString)
    {
        _connectionString = connectionString;
    }


    protected async Task<SqliteConnection> GetConnection()
    {
        SqliteConnection connection = new(_connectionString.Value);
        if (_isInitialized)
        {
            return connection;
        }

        await InitializeAsync(connection);

        _isInitialized = true;

        return connection;
    }

    /// <summary>
    /// Initialize the database tables. This method is called 
    /// automatically (and only once) when the database is created.
    /// Connection should not be disposed in this method.
    /// </summary>
    /// <param name="connection">Connection used to run the query.</param>
    /// <returns>awaitable Task</returns>
    protected abstract Task InitializeAsync(SqliteConnection connection);




}
