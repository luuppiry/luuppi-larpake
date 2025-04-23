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
    readonly UserService _service;
    readonly IRefreshTokenDatabase _refreshTokenDb;
    readonly IExternalIntegrationService _userInfoService;

    public UsersController(
        IUserDatabase db,
        UserService service,
        IClaimsReader claimsReader,
        IRefreshTokenDatabase refreshTokenDb,
        IExternalIntegrationService userInfoService,
        ILogger<UsersController> logger) : base(claimsReader, logger)
    {
        _db = db;
        _service = service;
        _refreshTokenDb = refreshTokenDb;
        _userInfoService = userInfoService;
    }


    [HttpGet]
    [RequiresPermissions(Permissions.ReadRawUserInfomation)]
    [ProducesResponseType<QueryDataGetDto<UserGetDto>>(200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryOptions options, CancellationToken token)
    {
        var users = await _service.GetFullUsers(options, token);
        if (users.IsError)
        {
            return FromError(users);
        }

        // Map to result
        var result = new QueryDataGetDto<UserGetDto>
        {
            Data = (UserGetDto[])users
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
        Result<UserGetDto>? record = await _service.GetFullUser(userId, token);
        return record.ToActionResult(
            ok: Ok,
            error: FromError
        );
    }

    [HttpGet("{userId}/reduced")]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType<ReducedUserGetDto>(200)]
    [ProducesResponseType(404)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> GetCommonUserInfo(Guid userId, CancellationToken token)
    {
        Result<UserGetDto>? record = await _service.GetFullUser(userId, token);

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
        Result<UserGetDto> record = await _service.GetFullUser(authorId, token);
        return record.ToActionResult(
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
            return BadRequest("UserId must be provided.", error: ErrorCode.EmptyId);
        }


        // Validate user is not changing own permissions or setting higher than admin
        Guid authorId = GetRequestUserId();
        if (targetId == authorId)
        {
            _logger.LogInformation("User {id} tried to change own permissions.", targetId);
            return BadRequest("Cannot change own permissions.", error: ErrorCode.SelfActionInvalid);
        }
        if (dto.Permissions.IsMoreThan(Permissions.Admin))
        {
            _logger.LogInformation("User {id} tried to set permissions higher than admin.", targetId);
            return BadRequest("Setting permissions higher than admin are forbidden in runtime.", error: ErrorCode.ActionNotAllowedInRuntime);
        }

        // Validate author exists
        DbUser? author = await _db.GetByUserId(authorId);

        if (author is null)
        {
            _logger.LogError("Authorized user {id} not found in database, " +
                "token might have leaked and needs immidiate actions, revoking!", authorId);

            await _refreshTokenDb.RevokeUserTokens(authorId);
            return InternalServerError("Invalid token.", error: ErrorCode.MalformedJWT);
        }
        if (author.IsAllowedToSet(dto.Permissions) is false)
        {
            _logger.LogInformation("User {id} tried to set permissions for higher role user.", authorId);
            return Unauthorized("Higher request role required.", error: ErrorCode.RequiresHigherRole);
        }
        if (author.Has(dto.Permissions) is false)
        {
            _logger.LogInformation("User {id} tried to set permissions higher than own.", authorId);
            return Unauthorized("Higher request role required.", error: ErrorCode.RequiresHigherRole);
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

        _logger.LogTrace("User {author} set permissions {value} for user {target}.",
            author, dto.Permissions, targetId);

        return result.ToActionResult(
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

        _logger.IfTrue(rowsAffected).LogTrace("User {id} revoked own permissions.", userId);
        _logger.IfFalse(rowsAffected).LogError("Failed to revoke permissions for user {id}.", userId);

        return rowsAffected.ToActionResult(
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
                "User {authorId} tried to delete user {userId} without correct permissions.",
                    GetRequestUserId(), userId);
            return FromError(roleValidationResult);
        }

        int rowsAffected = await _db.Delete(userId);

        _logger.IfPositive(rowsAffected).LogTrace("Deleted user {id}.", userId);

        return OkRowsAffected(rowsAffected);
    }



    private async Task<Result> RequireHigherAuthorRole(Guid targetId)
    {
        if (targetId == Guid.Empty)
        {
            return Error.BadRequest("UserId must be provided.", ErrorCode.EmptyId);
        }

        DbUser? target = await _db.GetByUserId(targetId);
        if (target is null)
        {
            return Error.NotFound("User not found.", ErrorCode.IdNotFound);
        }

        Permissions authorizedPermissions = GetRequestPermissions();
        if (authorizedPermissions.HasHigherRoleOrSudo(target.Permissions) is false)
        {
            return Error.Unauthorized("Higher role required.", ErrorCode.RequiresHigherRole);
        }
        return Result.Ok;
    }


}
