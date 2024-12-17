using LarpakeServer.Identity;
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

    Task<bool> IsSameRefreshToken(Guid id, string token);
    Task<bool> SetRefreshToken(Guid id, string token, DateTime expires);
    Task<int> RevokeRefreshToken(Guid id);
}