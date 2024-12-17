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
    public async Task<IActionResult> AddMembers(long groupId,FreshmanGroupMemberPostDto members)
    {
        Result<int> result = await _db.InsertMembers(groupId, members.MemberIds);
        if (result)
        {
            return OkRowsAffected((int)result);
        }
        return FromError(result);
    }

    [HttpPut("{groupId}")]
    public async Task<IActionResult> UpdateGroup(long groupId, [FromBody] FreshmanGroupPutDto dto)
    {
        var record = FreshmanGroup.MapFrom(dto);
        record.Id = groupId;
        Result<int> result = await _db.Update(record);
        if (result)
        {
            return OkRowsAffected((int)result);
        }
        return FromError(result);
    }

    [HttpDelete("{groupId}")]
    public async Task<IActionResult> DeleteGroup(long groupId)
    {
        int result = await _db.Delete(groupId);
        return OkRowsAffected(result);
    }

    [HttpDelete("{groupId}/members")]
    public async Task<IActionResult> DeleteMembers(long groupId, FreshmanGroupMemberDeleteDto members)
    {
        int result = await _db.DeleteMembers(groupId, members.MemberIds);
        return OkRowsAffected(result);
    }

}
