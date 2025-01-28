using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using System.Data.Common;
using System.Diagnostics;

namespace MigrationsService;
internal class QueryExecutionService
{
    readonly string _connectionString;
    private readonly ILogger<QueryExecutionService> _logger;
    readonly string _filenameStart;



    public QueryExecutionService(
        string connectionString, ILogger<QueryExecutionService>? logger = null)
    {
        string assemblyName = typeof(Program).Assembly.GetName().Name
            ?? throw new UnreachableException();

        _filenameStart = $"{assemblyName}.{MigrationsFolderName}.script-";
        _connectionString = connectionString;
        _logger = logger ?? NullLogger<QueryExecutionService>.Instance;
        InitializeMigrationTable();
        ReadyMigrations = GetReadyMigrations();
    }

    public const string MigrationsTableName = "migrations";
    public const string MigrationsFolderName = "Migrations";


    internal Dictionary<int, Migration> ReadyMigrations { get; private set; } = [];


    public void ExecuteEmbeddedMigrations(string[] filenames)
    {
        /* At this point all filenames should match RegexGen.IsSqlScriptFile()
         * and they are not validated anymore 
         * Migrations are only committed to database 
         * if all migrations are already ran or valid.
         */
        if (filenames.Length is 0)
        {
            throw new MigrationsFailedException("No SQL files found.");
        }



        int highestSequenceId = ReadyMigrations.Count is 0
            ? int.MinValue
            : ReadyMigrations.Keys.Max();

        _logger.LogInformation("Highest completed migration id is {id}.", highestSequenceId);

        Migration[] newMigrations = filenames
            .Select(FilenameToMigration)
            .OrderBy(x => x.SequenceId)
            .Where(x => x.SequenceId > highestSequenceId)
            .ToArray();
        if (newMigrations.Length is 0)
        {
            _logger.LogInformation("No new migrations found.");
            return;
        }

        _logger.LogInformation("Found {count} new migrations.", newMigrations.Length);

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            _logger.LogInformation("Starting migrations.");
            // Run migration
            foreach (var migration in newMigrations)
            {
                MigrateEmbedded(connection, migration.FileName);
            }

            // Add ready migrations to database
            SaveReadyMigrations(connection, newMigrations);

            foreach (var migration in newMigrations)
            {
                if (ReadyMigrations.TryAdd(migration.SequenceId, migration) is false)
                {
                    throw new MigrationsFailedException(migration.FileName,
                        "Failed to add migration to ready migrations.");
                }
            }
            _logger.LogInformation("Completed all {count} migrations.", newMigrations.Length);

            transaction.Commit();
        }
        catch (MigrationsFailedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MigrationsFailedException("Failed to execute migrations.", ex);
        }
    }

    private void InitializeMigrationTable()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        try
        {

            int rowsAffected = connection.Execute($"""
                CREATE TABLE IF NOT EXISTS {MigrationsTableName} (
                    {nameof(Migration.SequenceId)} INT,
                    {nameof(Migration.FileName)} TEXT,
                    PRIMARY KEY({nameof(Migration.SequenceId)})
                    );
                """);
            if (rowsAffected > 0)
            {
                _logger.LogInformation("Created migrations table.");
            }

        }
        catch (Exception e)
        {
            throw new MigrationsFailedException("Failed to create migrations table.", e);
        }
    }

    private Dictionary<int, Migration> GetReadyMigrations()
    {
        _logger.LogInformation("Retrieving ready migrations.");

        using var connection = new NpgsqlConnection(_connectionString);
        return connection.Query<Migration>($"""
            SELECT * FROM {MigrationsTableName};
            """)
            .ToDictionary(m => m.SequenceId, m => m);
    }
    private void SaveReadyMigrations(NpgsqlConnection connection, Migration[] newMigrations)
    {
        _logger.LogInformation("Saving {count} completed migrations.", newMigrations.Length);
        try
        {
            connection.Execute($"""
                INSERT INTO {MigrationsTableName}(
                    {nameof(Migration.SequenceId)},
                    {nameof(Migration.FileName)}
                )
                VALUES (
                    @{nameof(Migration.SequenceId)}, 
                    @{nameof(Migration.FileName)}
                );
                """, newMigrations);
        }
        catch (Exception ex)
        {
            throw new MigrationsFailedException("Failed to write ran migrations.", ex);
        }
    }

    private void MigrateEmbedded(DbConnection connection, string filename)
    {
        _logger.LogInformation("Running migration {filename}.", filename);

        using Stream file = typeof(Program).Assembly.GetManifestResourceStream(filename)
            ?? throw new MigrationsFailedException(filename, "File not found.");

        try
        {
            string sql = StreamToString(file);
            connection.Execute(sql);
        }
        catch (Exception e)
        {
            throw new MigrationsFailedException(filename, "Failed to migrate file.", e);
        }
    }

    private static string StreamToString(Stream input)
    {
        using var reader = new StreamReader(input);
        return reader.ReadToEnd();
    }

    private Migration FilenameToMigration(string filename)
    {
        // filenames are in format "AssemblyName.FolderName.script-NNNNNN filename.sql"

        if (filename.StartsWith(_filenameStart, StringComparison.OrdinalIgnoreCase) is false)
        {
            throw new MigrationsFailedException(filename, "Invalid name format, must be 'AssemblyName.FolderName.script-NNNNNN filename.sql'");
        }

        int seqStart = _filenameStart.Length + 1;
        int seqEnd = -1;
        for (int i = seqStart; i <= filename.Length; i++)
        {
            if (char.IsDigit(filename[i]) is false)
            {
                // First non digit
                seqEnd = i;
                break;
            }
        }
        if (seqEnd is 0)
        {
            throw new MigrationsFailedException(filename, "Sequence part too short, invalid name format.");
        }
        if (seqEnd is -1)
        {
            throw new MigrationsFailedException(filename, "Sequence part too long, invalid name format.");
        }

        int sequenceId = int.Parse(filename[seqStart..seqEnd]);
        return new Migration { FileName = filename, SequenceId = sequenceId };
    }
}
