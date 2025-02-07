﻿using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;
using FreshmanGroups = LarpakeServer.Models.GetDtos.Templates.QueryDataGetDto<LarpakeServer.Models.GetDtos.FreshmanGroupGetDto>;

namespace LarpakeServer.Controllers;


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GroupsController : ExtendedControllerBase
{
    readonly IGroupDatabase _db;

    public GroupsController(
        IGroupDatabase db,
        ILogger<GroupsController> logger,
        IClaimsReader claimsReader) : base(claimsReader, logger)
    {
        _db = db;
    }


    [HttpGet("own")]
    [RequiresPermissions(Permissions.CommonRead)]
    public async Task<IActionResult> GetOwnGroups([FromQuery] FreshmanGroupQueryOptions options)
    {
        /* User can here only see their own groups,
         * and members in those groups.
         * 
         * Searching by group name is limited to admin and up.
         */
        options.ContainsUser = GetRequestUserId();
        options.IncludeHiddenMembers = GetRequestPermissions().Has(Permissions.SeeHiddenMembers);

        if (GetRequestPermissions().Has(Permissions.Admin))
        {
            options.GroupName = null;
        }

        FreshmanGroups groups = await GetResultGroups(options);
        return Ok(groups);
    }

    [HttpGet]
    [RequiresPermissions(Permissions.Tutor)]
    public async Task<IActionResult> GetGroups([FromQuery] FreshmanGroupQueryOptions options)
    {
        /* Tutors can see all groups and members.
         */
        options.IncludeHiddenMembers = GetRequestPermissions().Has(Permissions.SeeHiddenMembers);

        FreshmanGroups groups = await GetResultGroups(options);
        return Ok(groups);
    }

    [HttpGet("{groupId}")]
    [RequiresPermissions(Permissions.Tutor)]
    public async Task<IActionResult> GetGroup(long groupId)
    {
        var record = await _db.GetGroup(groupId);
        if (record is null)
        {
            return IdNotFound();
        }
        var group = FreshmanGroupGetDto.From(record);
        return Ok(group);
    }

    [HttpGet("{groupId}/members")]
    [RequiresPermissions(Permissions.Tutor)]
    public async Task<IActionResult> GetMembers(long groupId)
    {
        var record = await _db.GetMembers(groupId);
        return record is null 
            ? IdNotFound() : Ok(new { Members = record });
    }

    [HttpPost]
    [RequiresPermissions(Permissions.CreateGroup)]
    public async Task<IActionResult> CreateGroup([FromBody] FreshmanGroupPostDto dto)
    {
        var record = FreshmanGroup.MapFrom(dto);
        Result<long> result = await _db.Insert(record);

        return result.MatchToResponse(
            ok: CreatedId,
            error: FromError);
    }

    [HttpPost("{groupId}/members")]
    [RequiresPermissions(Permissions.EditGroup)]
    public async Task<IActionResult> AddMembers(long groupId, GroupMemberIdCollection members)
    {
        Result validation = await RequireMemberOrAdmin(groupId);
        if (validation.IsError)
        {
            Guid userId = GetRequestUserId();
            _logger.LogInformation("Insufficent permissions for {userId} to add members to group {groupId}.", 
                userId, groupId);
            return FromError(validation);
        }

        Result<int> result = await _db.InsertMembers(groupId, members.MemberIds);
        return result.MatchToResponse(
            ok: OkRowsAffected,
            error: FromError
        );
    }

    [HttpPost("{groupId}/members/non-competing")]
    [RequiresPermissions(Permissions.EditGroup | Permissions.Admin)]
    public async Task<IActionResult> AddNonCompetingMembers(long groupId, NonCompetingMemberIdCollection members)
    {
        bool canAddHidden = GetRequestPermissions().Has(Permissions.SeeHiddenMembers);
        if (canAddHidden is false && members.Members.Any(x => x.IsHidden))
        {
            Error e = Error.BadRequest("Permission to see hidden members required to add one.");
            return FromError(e);
        }

        Result<int> result = await _db.InsertNonCompeting(groupId, members.Members);
        return result.MatchToResponse(
            ok: OkRowsAffected,
            error: FromError);
    }

    [HttpPut("{groupId}")]
    [RequiresPermissions(Permissions.EditGroup)]
    public async Task<IActionResult> UpdateGroup(long groupId, [FromBody] FreshmanGroupPutDto dto)
    {
        Result validation = await RequireMemberOrAdmin(groupId);
        if (validation.IsError)
        {
            Guid userId = GetRequestUserId();
            _logger.LogInformation("Insufficent permissions for {userId} to update group {groupId}.", 
                userId, groupId);
            return FromError(validation);
        }

        var record = FreshmanGroup.MapFrom(dto);
        record.Id = groupId;
        Result<int> result = await _db.Update(record);
        return result.MatchToResponse(
            ok: OkRowsAffected,
            error: FromError);
    }

    [HttpDelete("{groupId}/members")]
    [RequiresPermissions(Permissions.EditGroup)]
    public async Task<IActionResult> DeleteMembers(long groupId, [FromBody] GroupMemberIdCollection members)
    {
        Guid authorId = GetRequestUserId();

        // Validate user has permissions to delete in this group
        Result isValid = await RequireMemberOrAdmin(groupId);
        if (isValid.IsError)
        {
            _logger.LogInformation("Insufficent permissions for {userId} to delete members from group {groupId}.",
                authorId, groupId);
            return FromError(isValid);
        }

        // Delete members
        int result = await _db.DeleteMembers(groupId, members.MemberIds);
        if (result > 0)
        {
            foreach (var memberId in members.MemberIds)
            {
                _logger.LogInformation("Removed member {memberId} from group {groupId} by {userId}.",
                    memberId, groupId, authorId);
            }
        }
        return OkRowsAffected(result);
    }

    [HttpDelete("{groupId}")]
    [RequiresPermissions(Permissions.Admin)]
    public async Task<IActionResult> DeleteGroup(long groupId)
    {
        int result = await _db.Delete(groupId);
        
        _logger.IfPositive(result)
            .LogInformation("Group {groupId} deleted by {authorId}.", 
                groupId, GetRequestUserId());
      
        return OkRowsAffected(result);
    }




    private async Task<Result> RequireMemberOrAdmin(long groupId)
    {
        if (GetRequestPermissions().Has(Permissions.Admin))
        {
            // Is admin
            return Result.Ok;
        }

        // Is not admin
        var userId = GetRequestUserId();
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
        return Result.Ok;
    }

    private async Task<FreshmanGroups> GetResultGroups(FreshmanGroupQueryOptions options)
    {
        var records = await _db.GetGroups(options);
        var result = FreshmanGroups.MapFrom(records)
            .AppendPaging(options);
        return result;
    }
}
