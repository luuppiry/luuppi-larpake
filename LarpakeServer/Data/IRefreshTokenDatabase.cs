using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Data;
public interface IRefreshTokenDatabase
{
    Task<Result<bool>> Add(RefreshToken token);
    Task<bool> IsValid(Guid userId, string refreshToken);
    Task<int> Delete(Guid userId);
    Task ClearOldEntries();
}