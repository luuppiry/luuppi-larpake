using Microsoft.Data.Sqlite;

namespace LarpakeServer.Data.Sqlite;

public abstract class SqliteDbBase
{
    readonly SqliteDbBase[] _dependencies = [];
    readonly SqliteConnectionString _connectionString;
    bool _isInitialized = false;

    /// <summary>
    /// New database base class. 
    /// Handles referenced table loading and connection creation.
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="dependencies">Other database classes that this database instance table references.</param>
    protected SqliteDbBase(SqliteConnectionString connectionString, params SqliteDbBase[] dependencies)
    {
        Guard.ThrowIfNull(connectionString);
        Guard.ThrowIfNull(dependencies);

        _connectionString = connectionString;
        _dependencies = dependencies;
    }

    /// <summary>
    /// Get a connection to the database.
    /// Lazy loads the table and referenced tables.
    /// </summary>
    /// <returns><see cref="IDisposable"/> <see cref="SqliteConnection"/> for database actions.</returns>
    protected virtual async Task<SqliteConnection> GetConnection()
    {
        SqliteConnection connection = new(_connectionString.Value);
        if (_isInitialized)
        {
            return connection;
        }

        foreach (var dependency in _dependencies)
        {
            if (dependency._isInitialized is false)
            {
                await dependency.InitializeAsync(connection);
            }
        }

        await InitializeAsync(connection);

        _isInitialized = true;

        return connection;
    }

    /// <summary>
    /// Initialize the database tables. This method is called 
    /// automatically (and only once) when the database is created.
    /// Connection should not be disposed in this method.
    /// You should use the connection parameter to run the query.
    /// </summary>
    /// <param name="connection">Connection used to run the query.</param>
    /// <returns>awaitable Task</returns>
    protected abstract Task InitializeAsync(SqliteConnection connection);



    protected class SelectQuery
    {
        readonly StringBuilder _builder = new();
        bool hasWhere = false;

        /// <summary>
        /// Same as <see cref="StringBuilder.AppendLine(string?)"/>
        /// </summary>
        /// <param name="line"></param>
        internal void AppendLine(string line) => _builder.AppendLine(line);

        /// <summary>
        /// Append a condition line to the query.
        /// Automatically chooses whether to append WHERE or AND if already conditions appended.
        /// </summary>
        /// <param name="condition"></param>
        internal void AppendConditionLine(string condition)
        {
            if (hasWhere is false)
            {
                _builder.Append("WHERE ");
                hasWhere = true;
            }
            else
            {
                _builder.Append("AND ");
            }
            _builder.AppendLine(condition);
        }
        internal string Build()
        {
            return _builder.ToString();
        }

        public override string ToString() => Build();
    }

}
