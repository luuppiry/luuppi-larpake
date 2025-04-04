using LarpakeServer.Data;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.External;
using LarpakeServer.Models.GetDtos;

namespace LarpakeServer.Services;

public class UserService
{
    readonly IUserDatabase _db;
    readonly IExternalIntegrationService _service;
    readonly ILogger<UserService> _logger;

    public UserService(IUserDatabase db, IExternalIntegrationService service, ILogger<UserService> logger)
    {
        _db = db;
        _service = service;
        _logger = logger;
    }


    public async Task<Result<UserGetDto>> GetFullUser(Guid userId, CancellationToken token)
    {
        User? user = await _db.GetByUserId(userId);
        if (user is null)
        {
            return Error.NotFound("User id not found", ErrorCode.IdNotFound);
        }
        if (user.EntraId is null)
        {
            return UserGetDto.From(user);
        }

        // Fetch information like username and names from external source
        Result<ExternalUserInformation> luuppiUser =
            await _service.PullUserInformationFromExternalSource(user.EntraId.Value, token);
        if (luuppiUser.IsError)
        {
            _logger.LogWarning("Failed to get user information from external source for user {id}.", userId);
            return (Error)luuppiUser;
        }
        UserGetDto result = UserGetDto.From(user);
        result.Append((ExternalUserInformation)luuppiUser);
        return result;
    }
}
