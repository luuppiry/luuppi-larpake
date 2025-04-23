using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.Collections;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.GetDtos.Templates;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;
using LarpakeServer.Services;
using System.ComponentModel.DataAnnotations;
using FreshmanGroups = LarpakeServer.Models.GetDtos.Templates.QueryDataGetDto<LarpakeServer.Models.GetDtos.FreshmanGroupGetDto>;

namespace LarpakeServer.Controllers;


[Authorize]
[ApiController]
[Route("api/groups")]
public class GroupsController : ExtendedControllerBase
{
    record InviteKeyMsgResponse(string InviteKey, string Message);
    record struct MembersResponse(Guid[] Members);

    readonly IGroupDatabase _db;
    readonly IUserDatabase _userDb;
    readonly UserService _userService;

    public GroupsController(
        IGroupDatabase db,
        IUserDatabase userDb,
        ILogger<GroupsController> logger,
        UserService userService,
        IClaimsReader claimsReader) : base(claimsReader, logger)
    {
        _db = db;
        _userDb = userDb;
        _userService = userService;
    }


    [HttpGet("own")]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType<FreshmanGroups>(200)]
    public async Task<IActionResult> GetOwnGroups([FromQuery] FreshmanGroupQueryOptions options)
    {
        /* User can here only see their own groups,
         * and members in those groups.
         * 
         * Searching by group name is limited to admin and up.
         */
        options.ContainsUser = GetRequestUserId();
        options.IncludeHiddenMembers = GetRequestPermissions().Has(Permissions.SeeHiddenMembers)
            && options.IncludeHiddenMembers;

        if (GetRequestPermissions().Has(Permissions.Admin))
        {
            options.GroupName = null;
        }

        FreshmanGroups groups = await GetResultGroups(options);
        return Ok(groups);
    }

    [HttpGet]
    [RequiresPermissions(Permissions.Tutor)]
    [ProducesResponseType<FreshmanGroups>(200)]
    public async Task<IActionResult> GetGroups([FromQuery] FreshmanGroupQueryOptions options)
    {
        /* Tutors can see all groups and members.
         */
        options.IncludeHiddenMembers = GetRequestPermissions().Has(Permissions.SeeHiddenMembers)
            && options.IncludeHiddenMembers;

        FreshmanGroups groups = await GetResultGroups(options);
        return Ok(groups);
    }


    [HttpGet("{key}/join")]
    [ProducesResponseType<GroupInfo>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCommonInfo(string key)
    {
        GroupInfo? group = await _db.GetGroupByInviteKey(key);
        return group is null ? NotFound() : Ok(group);
    }




    [HttpGet("{groupId}")]
    [RequiresPermissions(Permissions.Tutor)]
    [ProducesResponseType<FreshmanGroup>(200)]
    [ProducesResponseType(404)]
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

    [HttpGet("{groupId}/member-ids")]
    [RequiresPermissions(Permissions.Tutor)]
    [ProducesResponseType<MembersResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMembers(long groupId)
    {
        var members = await _db.GetMemberIds(groupId);
        return members is null
            ? IdNotFound() : Ok(new MembersResponse(members));
    }







    [HttpGet("members")]
    [RequiresPermissions(Permissions.CommonRead)]
    public async Task<IActionResult> GetMembers([Required][FromQuery] long[] groupIds, CancellationToken token)
    {
        bool includeHidden = GetRequestPermissions().Has(Permissions.SeeHiddenMembers);
        Guid userId = GetRequestUserId();

        // Search is limited to groups containing user and groups defined
        RawGroupMemberCollection[] groups = await _db.GetMembers(groupIds, userId, includeHidden);


        // Combine all ids into a single distinct array
        Guid[] userIds = new HashSet<Guid>(groups.SelectMany(x => x.Members.Concat(x.Tutors))).ToArray();

        // Query
        var fullUsers = await _userService.GetFullUsers(new UserQueryOptions
        {
            UserIds = userIds,
        }, token);

        if (fullUsers.IsError)
        {
            return FromError(fullUsers);
        }

        Dictionary<Guid, UserGetDto> fullUsersDict = ((UserGetDto[])fullUsers).ToDictionary(x => x.Id);
        List<GroupMemberCollection> result = [];
        foreach (var group in groups)
        {
            result.Add(new GroupMemberCollection
            {
                GroupId = group.GroupId,
                Tutors = Map(group.Tutors),
                Members = Map(group.Members),
            });
        }

        UserGetDto[] Map(List<Guid> ids)
        {
            return ids.Select(id =>
            {
                if (fullUsersDict.TryGetValue(id, out var user))
                {
                    user.EntraId = null;
                    user.EntraUsername = null;
                    return user;
                }
                return null!;   // Filtered out on next step
            })
            .Where(x => x is not null)
            .ToArray();
        }

        return Ok(new QueryDataGetDto<GroupMemberCollection>
        {
            Data = result.ToArray()
        });
    }





    [HttpGet("{groupId}/invite")]
    [RequiresPermissions(Permissions.EditGroup)]
    [ProducesResponseType<InviteKeyMsgResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetInviteKey(long groupId)
    {
        var key = await _db.GetInviteKey(groupId);
        return key is null
            ? IdNotFound() : Ok(new InviteKeyMsgResponse((string)key, "Key valid until new generated."));
    }

    [HttpPost("{groupId}/invite/refresh")]
    [RequiresPermissions(Permissions.EditGroup)]
    [ProducesResponseType<InviteKeyMsgResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RefreshInviteKey(long groupId)
    {
        var key = await _db.RefreshInviteKey(groupId);
        return key is null
            ? IdNotFound()
            : Ok(new InviteKeyMsgResponse((string)key, "Old invite keys revoked."));
    }


    [HttpPost]
    [RequiresPermissions(Permissions.CreateGroup)]
    [ProducesResponseType(typeof(LongIdResponse), 201)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> CreateGroup([FromBody] FreshmanGroupPostDto dto)
    {
        var record = FreshmanGroup.MapFrom(dto);
        Result<long> result = await _db.Insert(record);

        return result.ToActionResult(
            ok: CreatedId,
            error: FromError);
    }

    [HttpPost("{key}/join")]    // No permissions required
    [ProducesResponseType<RowsAffectedResponse>(200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> JoinByInvite([Required] string key)
    {
        // Anyone authenticated should be able to join
        Guid userId = GetRequestUserId();

        // Join
        Result<int> joined = await _db.InsertMemberByInviteKey(key, userId);
        if (joined.IsError)
        {
            _logger.LogInformation("User {userId} failed to join group by invite key {key}.", userId, key);
            return FromError(joined);
        }

        // Joined successfully, give common read permissions
        Result<int> permitted = await _userDb.AppendPermissions(userId, Permissions.Freshman);
        if (permitted.IsError)
        {
            _logger.LogError("User {userId} did not get common read after group join.", userId);

            Error error = Error.InternalServerError($"""
                Group joined successfully,
                but failed to give common read permissions.
                Contact system admin. 
                """)
                .WithInner((Error)permitted);

            return FromError(error);
        }
        return OkRowsAffected(permitted);
    }



    [HttpPost("{groupId}/members")]
    [RequiresPermissions(Permissions.EditGroup)]
    [ProducesResponseType<RowsAffectedResponse>(200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
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
        return result.ToActionResult(
            ok: OkRowsAffected,
            error: FromError
        );
    }

    [HttpPost("{groupId}/members/non-competing")]
    [RequiresPermissions(Permissions.EditGroup | Permissions.Admin)]
    [ProducesResponseType<RowsAffectedResponse>(200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> AddNonCompetingMembers(long groupId, NonCompetingMemberIdCollection members)
    {
        bool canAddHidden = GetRequestPermissions().Has(Permissions.SeeHiddenMembers);
        if (canAddHidden is false && members.Members.Any(x => x.IsHidden))
        {
            Error e = Error.BadRequest("Permission to see hidden members required to add one.");
            return FromError(e);
        }

        Result<int> result = await _db.InsertNonCompeting(groupId, members.Members);
        return result.ToActionResult(
            ok: OkRowsAffected,
            error: FromError);
    }

    [HttpPut("{groupId}")]
    [RequiresPermissions(Permissions.EditGroup)]
    [ProducesResponseType<RowsAffectedResponse>(200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
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
        return result.ToActionResult(
            ok: OkRowsAffected,
            error: FromError);
    }

    [HttpDelete("{groupId}/members")]
    [RequiresPermissions(Permissions.EditGroup)]
    [ProducesResponseType<RowsAffectedResponse>(200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
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
    [ProducesResponseType<RowsAffectedResponse>(200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
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
        var members = await _db.GetMemberIds(groupId);
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
