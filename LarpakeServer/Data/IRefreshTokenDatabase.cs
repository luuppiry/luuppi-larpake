using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Data;
public interface IRefreshTokenDatabase
{
    /// <summary>
    /// Adds a refresh token to the database.
    /// </summary>
    /// <param name="token"></param>
    /// <returns>Successful result if success, otherwise false</returns>
    Task<Result> Add(RefreshToken token);

    /// <summary>
    /// Check if the refresh token is valid against the database.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="refreshToken"></param>
    /// <returns><see cref="RefreshTokenValidationResult"/> that represents succes or invalidation.</returns>
    Task<RefreshTokenValidationResult> IsValid(Guid userId, string refreshToken);

    /// <summary>
    /// Revoke all tokens from specific token family (same refresh token chain).
    /// </summary>
    /// <param name="tokenFamilyId"></param>
    /// <returns>Number of rows affected.</returns>
    Task<int> RevokeFamily(Guid tokenFamilyId);

    /// <summary>
    /// Invalidate all tokens from specific user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>Number of rows affected.</returns>
    Task<int> RevokeUserTokens(Guid userId);

    /// <summary>
    /// Clear all expired refresh tokens.
    /// </summary>
    /// <returns>Number of rows affected</returns>
    Task<int> ClearOldEntries();


}