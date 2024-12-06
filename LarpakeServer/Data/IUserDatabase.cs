using LarpakeServer.Models;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Data;

public interface IUserDatabase
{
    Task<User[]> Get(UserQueryOptions options);
    Task<User?> Get(Guid id);
    Task<Result<Guid>> Insert(User record);
    Task<Result<int>> Update(User record);
    Task<Result<int>> UpdatePermissions(Guid id, Permissions permissions);
    Task<int> Delete(Guid id);
}