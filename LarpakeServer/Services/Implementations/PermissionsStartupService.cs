using LarpakeServer.Data;
using LarpakeServer.Identity;
using Microsoft.Extensions.Options;

namespace LarpakeServer.Services.Implementations;

public class PermissionsStartupService : IHostedService
{
    readonly PermissionsOptions _options;
    readonly IUserDatabase _database;
    readonly ILogger<PermissionsStartupService> _logger;

    public PermissionsStartupService(
        IOptions<PermissionsOptions> options,
        IUserDatabase database,
        ILogger<PermissionsStartupService> logger)
    {
        _options = options.Value;
        _database = database;
        _logger = logger;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.SetOnStartup is false)
        {
            _logger.LogInformation("Permissions startup service is disabled.");
            return;
        }

        _logger.LogInformation("Setting {count} permissions.",
            _options.GetTargetCount());

        // Sudo permissions
        foreach (Guid userId in _options.Sudo)
        {
            await SetPermissions(userId, Permissions.Sudo);
        }

        // Admin permissions
        foreach (Guid userId in _options.Admin)
        {
            await SetPermissions(userId, Permissions.Admin);
        }

        // Specific permissions values
        foreach ((Guid userId, Permissions permission) in _options.Special)
        {
            await SetPermissions(userId, permission);
        }

        _logger.LogInformation("Startup permission configuration completed successfully.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task SetPermissions(Guid userId, Permissions permission)
    {
        if (userId == Guid.Empty)
        {
            throw new InvalidOperationException("Cannot set permissions for null user.");
        }

        Result<int> result = await _database.SetPermissions(userId, permission);
        if (result.IsOk)
        {
            return;
        }

        // Failed
        _logger.LogError("Failed to set permissions for user {userId}. Error: {error}",
            userId, (Error)result);

        if (_options.FailIfNotFound)
        {
            throw new InvalidOperationException($"Failed to set permissions for user '{userId}'.");
        }
    }
}
