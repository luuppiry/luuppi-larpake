using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Helpers.Generic;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
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
    readonly IClaimsReader _reader;

    public UsersController(
        IUserDatabase db,
        IClaimsReader reader,
        ILogger<UsersController> logger) : base(logger)
    {
        _db = db;
        _reader = reader;
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

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] UserPostDto dto)
    {
        // TODO: This should be handled differently when I know how
        // Might move this to authentication controller

        var record = DbUser.MapFrom(dto);
        Result<Guid> result = await _db.Insert(record);
        if (result)
        {
            _logger.LogInformation("Created new user {id}", (Guid)result);
            return CreatedId((Guid)result);
        }
        return FromError(result);
    }




    [HttpPut("{userId}")]
    [RequiresPermissions(Permissions.UpdateUserInformation)]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UserPutDto dto)
    {
        // Validate request author role is higher than target role
        Result roleValidationResult = await IsRequestAuthorInHigherRole(userId);
        if (roleValidationResult.IsError)
        {
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



    [HttpPut("{userId}/permissions")]
    public async Task<IActionResult> UpdateUserPermissions(Guid userId, [FromBody] UserPermissionsPutDto dto)
    {
        /* Validate roles (Author is always validated from database, not only from JWT)
         * - Request author must have matching permission to set target permission value.
         * - Request author must also have at least same permissions to be updated. 
         * - User cannot change their own permissions.
         */

        if (userId == Guid.Empty)
        {
            return BadRequest("UserId must be provided.");
        }

        Guid authorId = _reader.ReadAuthorizedUserId(Request);
        if (userId == authorId)
        {
            _logger.LogInformation("User {id} tried to change own permissions.", userId);
            return BadRequest("Cannot change own permissions.");
        }

        DbUser? author = await _db.Get(authorId);
        if (author is null)
        {
            _logger.LogError("Authorized user {id} not found in database, " +
                "token might have leaked and needs immidiate actions!", authorId);
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

        // Update
        Result<int> result = await _db.UpdatePermissions(userId, dto.Permissions);

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
        Result roleValidationResult = await IsRequestAuthorInHigherRole(userId);
        if (roleValidationResult.IsError)
        {
            _logger.LogInformation("User {id} tried to delete user {userId} without correct permission.", 
                _reader.ReadAuthorizedUserId(Request), userId);
            return FromError(roleValidationResult);
        }

        int rowsAffected = await _db.Delete(userId);
        if (rowsAffected > 0)
        {
            _logger.LogInformation("Deleted user {id}.", userId);
        }
        return OkRowsAffected(rowsAffected);
    }



    private async Task<Result> IsRequestAuthorInHigherRole(Guid targetId)
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

        Permissions authorizedPermissions = _reader.ReadAuthorizedUserPermissions(Request);
        if (authorizedPermissions.HasHigherRoleOrSudo(target.Permissions) is false)
        {
            return Error.Unauthorized("Higher role required.");
        }
        return Result.Ok;
    }
}
