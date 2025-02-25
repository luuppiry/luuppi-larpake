using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;

public interface IUserDatabase
{
    Task<User[]> Get(UserQueryOptions options);
    Task<User?> GetByUserId(Guid id);
    Task<User?> GetByEntraId(Guid entraId);
    Task<Result<Guid>> Insert(User record);
    Task<Result<int>> Update(User record);
    Task<Result<int>> SetPermissions(Guid id, Permissions permissions);
    Task<Result<int>> SetPermissionsByEntra(Guid id, Permissions permissions);
    Task<Result<int>> AppendPermissions(Guid id, Permissions permissions);
    Task<Result<int>> AppendPermissionsByEntra(Guid id, Permissions permissions);
    Task<int> Delete(Guid id);
}