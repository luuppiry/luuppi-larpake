using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using System.Diagnostics;
using System.Security.Cryptography;

namespace LarpakeServer.Data.PostgreSQL;

public class RefreshTokenDatabase(NpgsqlConnectionString connectionString, ILogger<RefreshTokenDatabase> logger)
    : PostgresDb(connectionString, logger), IRefreshTokenDatabase
{
    public async Task<Result> Add(RefreshToken token)
    {
        if (token.UserId == Guid.Empty)
        {
            return Error.BadRequest("User id cannot be empty.");
        }
        if (token.Token is null)
        {
            return Error.BadRequest("Refresh token cannot be null.");
        }
        if (token.InvalidAt < DateTime.UtcNow)
        {
            Logger.LogWarning("Cannot insert invalid token, (invalidated at {time}).", token.InvalidAt);
            throw new InvalidOperationException("Cannot insert already invalid token.");
        }

        // Hash the refresh token and create new family if needed
        var hashed = new RefreshToken
        {
            UserId = token.UserId,
            Token = ComputeSHA256Hash(token.Token),
            InvalidAt = token.InvalidAt,
            TokenFamily = token.TokenFamily == Guid.Empty ? Guid.NewGuid() : token.TokenFamily
        };

        using var connection = GetConnection();
        try
        {
            await connection.ExecuteAsync($"""
            INSERT INTO refresh_tokens (
                user_id,
                token, 
                token_family,
                invalid_at
            )
            VALUES (
                @{nameof(RefreshToken.UserId)}, 
                @{nameof(RefreshToken.Token)}, 
                @{nameof(RefreshToken.TokenFamily)}, 
                @{nameof(RefreshToken.InvalidAt)}
            );
            """, hashed);

            return Result.Ok;
        }
        catch (Exception ex)
        {
            // TODO: Handle exception
            Logger.LogError("Failed to insert refresh token {msg}.", ex.Message);
            throw;
        }
    }

    public async Task<RefreshTokenValidationResult> IsValid(Guid userId, string refreshToken)
    {
        // Get hash
        string hash = ComputeSHA256Hash(refreshToken);

        using var connection = GetConnection();
        var token = await connection.QueryFirstOrDefaultAsync<RefreshToken>($"""
            SELECT 
                user_id, 
                token,
                token_family,
                invalid_at,
                invalidated_at,
                created_at
            FROM refresh_tokens
            WHERE user_id = @{nameof(userId)} 
                AND token = @{nameof(hash)}
            LIMIT 1;
            """, new { userId, hash });

        if (token is null)
        {
            Logger.LogInformation("Token with hash {hashStart}*** not found for user {id}.",
                SafeSlicer.Slice(hash, 10), userId);
            return RefreshTokenValidationResult.Invalid;
        }

        /* If token is already invalidated,
         * All other tokens created from it should be invalidated as well.
         */
        bool isTimeInvalidated = token.InvalidAt < DateTime.UtcNow;
        bool isUserInvalidated = token.InvalidatedAt is not null;
        if (isTimeInvalidated || isUserInvalidated)
        {
            Logger.LogWarning("Token with hash {hashStart}*** is invalidated for user {id}, revoking.",
                SafeSlicer.Slice(hash, 10), userId);

            await RevokeFamily(token.TokenFamily);
            return RefreshTokenValidationResult.Invalid;
        }

        // Invalidate token after use
        int rowsAffected = await connection.ExecuteAsync($"""
            UPDATE refresh_tokens
            SET 
                invalidated_at = NOW()
            WHERE user_id = @{nameof(userId)}
                AND token = @{nameof(hash)};
            """, new { userId, hash });

        if (rowsAffected is 0)
        {
            throw new UnreachableException("Failed to invalidate token.");
        }

        return new RefreshTokenValidationResult(token.TokenFamily);
    }

    public Task<int> RevokeUserTokens(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<int> RevokeFamily(Guid tokenFamilyId)
    {
        throw new NotImplementedException();
    }

    public Task<int> ClearOldEntries()
    {
        throw new NotImplementedException();
    }

    private static string ComputeSHA256Hash(string refreshToken)
    {
        Guard.ThrowIfNull(refreshToken);

        byte[] tokenBytes = Encoding.UTF8.GetBytes(refreshToken);
        byte[] hash = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hash);
    }
}
