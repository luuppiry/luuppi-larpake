using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MigrationsService;

// Pull configuration
ConfigurationBuilder builder = new();
IConfiguration config = builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .Build();

// Create logger
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<QueryExecutionService>();

// Get all embedded SQL file names
var sqlFileNames = typeof(Program).Assembly
    .GetManifestResourceNames()
    .ToArray();

// Execute migrations

string connectionString = Environment.GetEnvironmentVariable("PG_CONNECTION_STRING") ??
    config.GetConnectionString("Default")!;

logger.LogInformation(connectionString);

QueryExecutionService service = new(connectionString, logger);
service.ExecuteEmbeddedMigrations(sqlFileNames);



