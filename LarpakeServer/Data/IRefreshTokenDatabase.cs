using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Data;
public interface IRefreshTokenDatabase
{
    Task<Result<bool>> Add(RefreshToken token);

    /// <summary>
    /// Check if the refresh token is valid for the user.
    /// Token is revoked in the database during the validation.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="refreshToken"></param>
    /// <returns>True if token is valid and new token can be given to the user, otherwise false.</returns>
    Task<bool> IsValid(Guid userId, string refreshToken);
    Task<int> Delete(Guid userId);
    Task ClearOldEntries();
}