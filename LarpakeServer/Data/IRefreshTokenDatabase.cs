using LarpakeServer.Identity;
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
    /// <param name="userId">Token owner id.</param>
    /// <param name="refreshToken">Unhashed refresh token</param>
    /// <returns><see cref="RefreshTokenValidationResult"/> that represents success or invalidation.</returns>
    Task<RefreshTokenValidationResult> Validate(string refreshToken, Guid userId);

    /// <summary>
    /// Check if the refresh token is valid against the database.
    /// </summary>
    /// <param name="refreshToken">Unhashed refresh token.</param>
    /// <returns><see cref="RefreshTokenValidationResult"/> that represents success or invalidation.</returns>
    Task<RefreshTokenValidationResult> Validate(string refreshToken);

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