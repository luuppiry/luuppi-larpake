using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Diagnostics;
using System.Security.Cryptography;

namespace LarpakeServer.Data.PostgreSQL;

public class RefreshTokenDatabase : PostgresDb, IRefreshTokenDatabase
{
    readonly LarpakeIdOptions _options;

    public RefreshTokenDatabase(
        NpgsqlConnectionString connectionString, 
        ILogger<RefreshTokenDatabase> logger,
        IOptions<LarpakeIdOptions> options)
        : base(connectionString, logger)
    {
        _options = options.Value;
    }


    public async Task<Result> Add(RefreshToken token)
    {
        if (token.UserId == Guid.Empty)
        {
            return Error.BadRequest("Id required.", ErrorCode.NullId);
        }
        if (token.Token is null)
        {
            Logger.LogError("Null refresh token was attempted to pass to database.");
            throw new InvalidOperationException("Refresh token cannot be null");
        }
        if (token.InvalidAt < DateTime.UtcNow)
        {
            Logger.LogWarning("Cannot insert invalid token, (invalidated at {time}).", token.InvalidAt);
            throw new InvalidOperationException("Cannot insert already invalidated token.");
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
        catch (NpgsqlException ex) when (ex.SqlState == PostgresError.UniqueViolation)
        {
            Logger.LogWarning("Refresh token hash conflict");
            return Error.InternalServerError("Failed to generate unique refresh token", ErrorCode.KeyGenFailed);
        }
        catch (NpgsqlException ex) when (ex.SqlState == PostgresError.ForeignKeyViolation)
        {
            Logger.LogWarning("Refresh token user {id} not found", hashed.UserId);
            return Error.InternalServerError("User not found", ErrorCode.UserNotFound);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unhandled exception during refresh token insertion.");
            throw;
        }
    }

    public async Task<RefreshTokenValidationResult> Validate(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            Logger.LogWarning("Empty refresh token provided.");
            return RefreshTokenValidationResult.Invalid;
        }

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
            WHERE token = @{nameof(hash)}
            LIMIT 1;
            """, new { hash });

        if (token is null)
        {
            Logger.LogInformation("Token with hash {hashStart}*** not found.",
                SafeSlicer.Slice(hash, 10));
            return RefreshTokenValidationResult.Invalid;
        }

        // Validate and invalidate after single use
        return await ValidateAndRevoke(token, connection);
    }





    public async Task<RefreshTokenValidationResult> Validate(string refreshToken, Guid userId)
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

        // Validate and invalidate after single use
        return await ValidateAndRevoke(token, connection);
    }

    public async Task<int> RevokeUserTokens(Guid userId)
    {
        Logger.LogInformation("Revoking all tokens for user {id}.", userId);

        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE refresh_tokens
                SET invalidated_at = NOW()
            WHERE user_id = @{nameof(userId)};
            """, new { userId });
    }

    public async Task<int> RevokeFamily(Guid tokenFamilyId)
    {
        Logger.LogInformation("Revoking all tokens in token family {id}.", tokenFamilyId);

        using var connection = GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE refresh_tokens
                SET invalidated_at = NOW()
            WHERE token_family = @{nameof(tokenFamilyId)};
            """, new { tokenFamilyId });
    }

    public async Task<int> ClearOldEntries()
    {
        using var connection = GetConnection();
        int rowsAffected = await connection.ExecuteAsync($"""
            DELETE FROM refresh_tokens WHERE invalid_at < NOW();
            """);

        Logger.LogTrace ("Cleared {count} expired refresh token entries.", rowsAffected);
        return rowsAffected;
    }

    private static string ComputeSHA256Hash(string refreshToken)
    {
        Guard.ThrowIfNull(refreshToken);

        byte[] tokenBytes = Encoding.UTF8.GetBytes(refreshToken);
        byte[] hash = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hash);
    }

    private async Task<RefreshTokenValidationResult> ValidateAndRevoke(RefreshToken token, NpgsqlConnection connection)
    {
        if (token is null)
        {
            return RefreshTokenValidationResult.Invalid;
        }

        /* If token is already invalidated,
        * All other tokens created from it should be invalidated as well.
        */

        
        DateTime invalidatedOffset = DateTime.UtcNow.AddSeconds(_options.RefreshTokenExpirationCooldownSeconds);

        bool isTimeInvalidated = token.InvalidAt < DateTime.UtcNow;
        bool isUserInvalidated = token.InvalidatedAt is not null && DateTime.UtcNow > invalidatedOffset;
        if (isTimeInvalidated || isUserInvalidated)
        {
            Logger.LogWarning("Token with hash {hashStart}*** is invalidated for user {id}, revoking family, double use.",
                SafeSlicer.Slice(token.Token, 10), token.UserId);

            await RevokeFamily(token.TokenFamily);
            return RefreshTokenValidationResult.Invalid;
        }

        // Invalidate token after use
        int rowsAffected = await connection.ExecuteAsync($"""
            UPDATE refresh_tokens
            SET 
                invalidated_at = NOW()
            WHERE token = @{nameof(token.Token)};
            """, token);

        if (rowsAffected is 0)
        {
            throw new UnreachableException("Failed to invalidate token.");
        }

        Logger.LogTrace("Token {token}**** is valid for user {id}.",
            SafeSlicer.Slice(token.Token, 10), token.UserId);

        return new RefreshTokenValidationResult(token.TokenFamily, token.UserId);
    }

}
