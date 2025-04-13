using LarpakeServer.Data.Helpers;
using LarpakeServer.Models.Collections;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.QueryOptions;
using static LarpakeServer.Controllers.GroupsController;

namespace LarpakeServer.Data;

public interface IGroupDatabase
{
    Task<FreshmanGroup[]> GetGroups(FreshmanGroupQueryOptions options);
    Task<FreshmanGroup[]> GetGroupsMinimized(FreshmanGroupQueryOptions options);
    Task<FreshmanGroup?> GetGroup(long id);
    Task<Guid[]?> GetMemberIds(long id);
    Task<Result<long>> Insert(FreshmanGroup record);
    Task<Result<int>> InsertMembers(long id, Guid[] members);
    Task<Result<int>> InsertNonCompeting(long groupId, NonCompetingMember[] members);
    Task<Result<int>> Update(FreshmanGroup record);
    Task<int> Delete(long id);
    Task<int> DeleteMembers(long id, Guid[] members);
    Task<Result<string>> GetInviteKey(long groupId);
    Task<Result<int>> InsertMemberByInviteKey(string inviteKey, Guid userId);
    Task<Result<string>> RefreshInviteKey(long groupId);
    Task<GroupInfo?> GetGroupByInviteKey(string key);
    Task<RawGroupMemberCollection[]> GetMembers(long[] groupIds, Guid userId, bool includeHidden);
}
