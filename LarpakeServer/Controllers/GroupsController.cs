using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.DeleteDtos;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Controllers;


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GroupsController : ExtendedControllerBase
{
    private readonly IFreshmanGroupDatabase _db;
    private readonly IClaimsReader _reader;

    public GroupsController(
        IFreshmanGroupDatabase db,
        ILogger<GroupsController> logger,
        IClaimsReader reader) : base(logger)
    {
        _db = db;
        _reader = reader;
    }

    [HttpGet]
    public async Task<IActionResult> GetGroups([FromQuery] FreshmanGroupQueryOptions options)
    {
        var records = await _db.Get(options);
        var result = FreshmanGroupsGetDto.MapFrom(records);
        result.CalculateNextPageFrom(options);
        return Ok(result);
    }

    [HttpGet("{groupId}")]
    public async Task<IActionResult> GetGroup(long groupId)
    {
        var record = await _db.Get(groupId);
        if (record is null)
        {
            return IdNotFound();
        }
        return Ok(FreshmanGroupGetDto.From(record));
    }

    [HttpGet("{groupId}/members")]
    public async Task<IActionResult> GetMembers(long groupId)
    {
        var record = await _db.GetMembers(groupId);
        if (record is null)
        {
            return IdNotFound();
        }
        return Ok(new { Members = record });
    }

    [HttpPost]
    [RequiresPermissions(Permissions.CreateGroup)]
    public async Task<IActionResult> CreateGroup([FromBody] FreshmanGroupPostDto dto)
    {
        var record = FreshmanGroup.MapFrom(dto);
        Result<long> result = await _db.Insert(record);

        if (result)
        {
            return CreatedId((long)result);
        }
        return FromError(result);
    }

    [HttpPost("{groupId}/members")]
    [RequiresPermissions(Permissions.EditGroup)]
    public async Task<IActionResult> AddMembers(long groupId, FreshmanGroupMemberPostDto members)
    {
        var isValid = await RequireMemberOrAdmin(groupId);
        if (isValid.IsError)
        {
            return FromError(isValid);
        }

        Result<int> result = await _db.InsertMembers(groupId, members.MemberIds);
        if (result)
        {
            return OkRowsAffected((int)result);
        }
        return FromError(result);
    }

    [HttpPut("{groupId}")]
    [RequiresPermissions(Permissions.EditGroup)]
    public async Task<IActionResult> UpdateGroup(long groupId, [FromBody] FreshmanGroupPutDto dto)
    {
        var isValid = await RequireMemberOrAdmin(groupId);
        if (isValid.IsError)
        {
            return FromError(isValid);
        }

        var record = FreshmanGroup.MapFrom(dto);
        record.Id = groupId;
        Result<int> result = await _db.Update(record);
        if (result)
        {
            return OkRowsAffected((int)result);
        }
        return FromError(result);
    }

    [HttpDelete("{groupId}/members")]
    [RequiresPermissions(Permissions.EditGroup)]
    public async Task<IActionResult> DeleteMembers(long groupId, FreshmanGroupMemberDeleteDto members)
    {
        var isValid = await RequireMemberOrAdmin(groupId);
        if (isValid.IsError)
        {
            return FromError(isValid);
        }

        int result = await _db.DeleteMembers(groupId, members.MemberIds);
        return OkRowsAffected(result);
    }

    [HttpDelete("{groupId}")]
    [RequiresPermissions(Permissions.Admin)]
    public async Task<IActionResult> DeleteGroup(long groupId)
    {
        int result = await _db.Delete(groupId);
        return OkRowsAffected(result);
    }




    private async Task<Result<bool>> RequireMemberOrAdmin(long groupId)
    {
        if (_reader.ReadAuthorizedUserPermissions(Request).Has(Permissions.Admin))
        {
            // Is admin
            return true;
        }

        // Is not admin
        var userId = _reader.ReadAuthorizedUserId(Request);
        var members = await _db.GetMembers(groupId);
        if (members is null)
        {
            // Group not found
            return Error.NotFound("Group not found");
        }
        if (members.Contains(userId) is false)
        {
            // Not a member
            return Error.Unauthorized("Must be admin or group member.");
        }
        // Is member
        return true;
    }
}
