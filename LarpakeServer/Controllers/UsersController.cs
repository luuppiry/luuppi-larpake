using System.Security.Cryptography.Pkcs;
using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.External;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.GetDtos.Templates;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;
using LarpakeServer.Services;
using DbUser = LarpakeServer.Models.DatabaseModels.User;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ExtendedControllerBase
{
    readonly IUserDatabase _db;
    readonly IRefreshTokenDatabase _refreshTokenDb;
    readonly IExternalIntegrationService _userInfoService;

    public UsersController(
        IUserDatabase db,
        IClaimsReader claimsReader,
        IRefreshTokenDatabase refreshTokenDb,
        IExternalIntegrationService userInfoService,
        ILogger<UsersController> logger) : base(claimsReader, logger)
    {
        _db = db;
        _refreshTokenDb = refreshTokenDb;
        _userInfoService = userInfoService;
    }





    [HttpGet]
    [RequiresPermissions(Permissions.ReadRawUserInfomation)]
    [ProducesResponseType(typeof(QueryDataGetDto<UserGetDto>), 200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryOptions options, CancellationToken token)
    {
        DbUser[] records = await _db.Get(options);

        // To dictionary
        Dictionary<Guid, UserGetDto> entraUsers = [];
        List<UserGetDto> nonEntraUsers = [];
        foreach (DbUser user in records)
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


        // Fetch external information
        Task<Result<ExternalUserInformation>>[] externalTasks = records
            .Where(x => x.EntraId is not null)
            .Select(x => _userInfoService.PullUserInformationFromExternalSource(x.EntraId!.Value, token))
            .ToArray();


        // Append external information
        foreach (Result<ExternalUserInformation> user in await Task.WhenAll(externalTasks))
        {
            if (user.IsError && ((Error)user).ApplicationErrorCode is ErrorCode.IdNotFound)
            {
                continue;
            }
            if (user.IsError)
            {
                return FromError(user);
            }
            var value = (ExternalUserInformation)user;
            if (entraUsers.TryGetValue(value.EntraId, out UserGetDto? userDto))
            {
                userDto.Append(value);
            }
        }


        // Map to result
        var result = new QueryDataGetDto<UserGetDto>
        {
            Data = nonEntraUsers.Concat(entraUsers.Values).ToArray()
        }
        .AppendPaging(options);

        return Ok(result);
    }


    [HttpGet("{userId}")]
    [RequiresPermissions(Permissions.ReadRawUserInfomation)]
    [ProducesResponseType(typeof(UserGetDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUser(Guid userId, CancellationToken token)
    {
        Result<UserGetDto>? record = await GetFullUser(userId, token);
        return record.MatchToResponse(
            ok: Ok,
            error: FromError
        );
    }

    [HttpGet("{userId}/reduced")]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType(typeof(ReducedUserGetDto), 200)]
    [ProducesResponseType(404)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> GetCommonUserInfo(Guid userId, CancellationToken token)
    {
        Result<UserGetDto>? record = await GetFullUser(userId, token);

        if (record.IsError)
        {
            return FromError(record);
        }

        ReducedUserGetDto reduced = ReducedUserGetDto.From((UserGetDto)record);
        return Ok(reduced);
    }





    [HttpGet("me")]
    [ProducesResponseType(typeof(UserGetDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMe(CancellationToken token)
    {
        Guid authorId = GetRequestUserId();
        Result<UserGetDto> record = await GetFullUser(authorId, token);
        return record.MatchToResponse(
            ok: Ok,
            error: FromError);
    }

    [HttpPut("{userId}")]
    [RequiresPermissions(Permissions.UpdateUserInformation)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UserPutDto dto)
    {
        // Validate request author role is higher than target role
        Result roleValidationResult = await RequireHigherAuthorRole(userId);
        if (roleValidationResult.IsError)
        {
            _logger.LogInformation("Denied user {id} update request for {targetId}.",
                GetRequestUserId(), userId);
            return FromError(roleValidationResult);
        }

        // Update
        DbUser record = DbUser.MapFrom(dto);
        record.Id = userId;

        Result<int> result = await _db.Update(record);
        if (result)
        {
            _logger.LogInformation("Updated user {id}.", userId);
            return OkRowsAffected((int)result);
        }
        return FromError(result);
    }



    [HttpPut("{targetId}/permissions")]
    [RequiresPermissions(Permissions.UpdateUserInformation)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> UpdateUserPermissions(Guid targetId, [FromBody] UserPermissionsPutDto dto)
    {
        /* Validate roles (Author is always validated from database, not only from JWT)
         * - Request author must have matching permission to set target permission value.
         * - Request author must also have at least same permissions to be updated. 
         * - User cannot change their own permissions.
         */

        if (targetId == Guid.Empty)
        {
            return BadRequest("UserId must be provided.");
        }


        // Validate user is not changing own permissions or setting higher than admin
        Guid authorId = GetRequestUserId();
        if (targetId == authorId)
        {
            _logger.LogInformation("User {id} tried to change own permissions.", targetId);
            return BadRequest("Cannot change own permissions.");
        }
        if (dto.Permissions.IsMoreThan(Permissions.Admin))
        {
            _logger.LogInformation("User {id} tried to set permissions higher than admin.", targetId);
            return BadRequest("Setting permissions higher than admin are forbidden in runtime.");
        }

        // Validate author exists
        DbUser? author = await _db.GetByUserId(authorId);

        if (author is null)
        {
            _logger.LogError("Authorized user {id} not found in database, " +
                "token might have leaked and needs immidiate actions, revoking!", authorId);

            await _refreshTokenDb.RevokeUserTokens(authorId);
            return InternalServerError("Invalid token.");
        }
        if (author.IsAllowedToSet(dto.Permissions) is false)
        {
            _logger.LogInformation("User {id} tried to set permissions for higher role user.", authorId);
            return Unauthorized("Higher request role required.");
        }
        if (author.Has(dto.Permissions) is false)
        {
            _logger.LogInformation("User {id} tried to set permissions higher than own.", authorId);
            return Unauthorized("Higher request role required.");
        }

        // Validate target exists
        DbUser? target = await _db.GetByUserId(targetId);
        if (target is null)
        {
            _logger.LogInformation("User {id} not found.", targetId);
            return IdNotFound();
        }


        // Do update
        Result<int> result = await _db.SetPermissions(targetId, dto.Permissions);


        _logger.LogInformation("User {author} set permissions {value} for user {target}.",
            author, dto.Permissions, targetId);

        return result.MatchToResponse(
                ok: OkRowsAffected,
                error: FromError
            );
    }


    [HttpDelete("own/permissions")]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> RevokeOwnPermissions()
    {
        /* Because user is making the request, he should exit in database.
         * Otherwise user is deleted and token still exists sometime.
         */
        Guid userId = GetRequestUserId();
        Result<int> rowsAffected = await _db.SetPermissions(userId, Permissions.None);

        _logger.IfTrue(rowsAffected).LogInformation("User {id} revoked own permissions.", userId);
        _logger.IfFalse(rowsAffected).LogError("Failed to revoke permissions for user {id}.", userId);

        return rowsAffected.MatchToResponse(
            ok: OkRowsAffected,
            error: FromError
        );
    }



    [HttpDelete("{userId}")]
    [RequiresPermissions(Permissions.DeleteUser)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        // Validate request author role is higher than target role
        Result roleValidationResult = await RequireHigherAuthorRole(userId);
        if (roleValidationResult.IsError)
        {
            _logger.LogInformation(
                "User {authorId} tried to delete user {userId} without correct permission.",
                    GetRequestUserId(), userId);
            return FromError(roleValidationResult);
        }

        int rowsAffected = await _db.Delete(userId);

        _logger.IfPositive(rowsAffected)
            .LogInformation("Deleted user {id}.", userId);

        return OkRowsAffected(rowsAffected);
    }



    private async Task<Result> RequireHigherAuthorRole(Guid targetId)
    {
        if (targetId == Guid.Empty)
        {
            return Error.BadRequest("UserId must be provided.");
        }

        DbUser? target = await _db.GetByUserId(targetId);
        if (target is null)
        {
            return Error.NotFound("User not found.");
        }

        Permissions authorizedPermissions = GetRequestPermissions();
        if (authorizedPermissions.HasHigherRoleOrSudo(target.Permissions) is false)
        {
            return Error.Unauthorized("Higher role required.");
        }
        return Result.Ok;
    }

    private async Task<Result<UserGetDto>> GetFullUser(Guid userId, CancellationToken token)
    {
        DbUser? user = await _db.GetByUserId(userId);
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
            await _userInfoService.PullUserInformationFromExternalSource(user.EntraId.Value, token);
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
