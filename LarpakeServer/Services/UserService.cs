using LarpakeServer.Data;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.External;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.QueryOptions;

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
        // Get user from database
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

    public async Task<Result<UserGetDto[]>> GetFullUsers(UserQueryOptions options, CancellationToken token)
    {
        User[] records = await _db.Get(options);

        // To dictionary
        Dictionary<Guid, UserGetDto> entraUsers = [];
        List<UserGetDto> nonEntraUsers = [];
        foreach (User user in records)
        {
            if (user.EntraId is null)
            {
                nonEntraUsers.Add(UserGetDto.From(user));
            }
            else
            {
                entraUsers[user.EntraId!.Value] = UserGetDto.From(user);
            }
        }

        // create external information fetch tasks
        Task<Result<ExternalUserInformation>>[] externalTasks = records
            .Where(x => x.EntraId is not null)
            .Select(x => _service.PullUserInformationFromExternalSource(x.EntraId!.Value, token))
            .ToArray();

        // Append external information
        await foreach (Task<Result<ExternalUserInformation>> user in Task.WhenEach(externalTasks))
        {
            if (user.IsFaulted)
            {
                _logger.LogWarning("Exception thrown during external user retrieval: {ex}.", user.Exception);
                return Error.InternalServerError("Exception thrown during user external user retrieval");
            }
            Result<ExternalUserInformation> taskResult = await user;
            if (taskResult.IsError && (Error)taskResult is { ApplicationErrorCode: ErrorCode.IdNotFound })
            {
                continue;
            }
            if (taskResult.IsError)
            {
                return (Error)taskResult;
            }

            var userInfo = (ExternalUserInformation)taskResult;
            if (entraUsers.TryGetValue(userInfo.EntraId, out UserGetDto? userDto))
            {
                userDto.Append(userInfo);
            }
        }
        return nonEntraUsers.Concat(entraUsers.Values).ToArray();
    }
}
