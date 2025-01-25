using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.GetDtos.MultipleItems;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;

using DbUser = LarpakeServer.Models.DatabaseModels.User;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ExtendedControllerBase
{
    readonly IUserDatabase _db;
    readonly IRefreshTokenDatabase _refreshTokenDb;

    public UsersController(
        IUserDatabase db,
        IClaimsReader claimsReader,
        IRefreshTokenDatabase refreshTokenDb,
        ILogger<UsersController> logger) : base(claimsReader, logger)
    {
        _db = db;
        _refreshTokenDb = refreshTokenDb;
    }



    [HttpGet]
    [RequiresPermissions(Permissions.ReadRawUserInfomation)]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryOptions options)
    {
        var records = await _db.Get(options);
        var result = UsersGetDto.MapFrom(records);
        result.SetNextPaginationPage(options);
        return Ok(result);
    }


    [HttpGet("{userId}")]
    [RequiresPermissions(Permissions.ReadRawUserInfomation)]
    public async Task<IActionResult> GetUser(Guid userId)
    {
        var record = await _db.Get(userId);
        if (record is null)
        {
            return NotFound();
        }
        return Ok(record);
    }


    [HttpPut("{userId}")]
    [RequiresPermissions(Permissions.UpdateUserInformation)]
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

        // Validate user is not changing own permissions
        Guid authorId = GetRequestUserId();
        if (targetId == authorId)
        {
            _logger.LogInformation("User {id} tried to change own permissions.", targetId);
            return BadRequest("Cannot change own permissions.");
        }

        // Validate author exists
        DbUser? author = await _db.Get(authorId);

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
        DbUser? target = await _db.Get(targetId);
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



    [HttpDelete("{userId}")]
    [RequiresPermissions(Permissions.DeleteUser)]
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

        DbUser? target = await _db.Get(targetId);
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


}
