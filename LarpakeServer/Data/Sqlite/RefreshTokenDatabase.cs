using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;

namespace LarpakeServer.Data.Sqlite;

public class RefreshTokenDatabase : SqliteDbBase, IRefreshTokenDatabase
{
    readonly ILogger<IRefreshTokenDatabase> _logger;

    public RefreshTokenDatabase(SqliteConnectionString connectionString,
        UserDatabase userDb,
        ILogger<RefreshTokenDatabase> logger) : base(connectionString, userDb)
    {
        _logger = logger;
    }


    public async Task<Result> Add(RefreshToken token)
    {
        // Validate token
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
            _logger.LogWarning("Cannot insert invalid token, (invalidated at {time}).", token.InvalidAt);
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

        // Save token
        using var connection = await GetConnection();
        try
        {
            await connection.ExecuteAsync($"""
            INSERT INTO RefreshTokens (
                {nameof(RefreshToken.UserId)}, 
                {nameof(RefreshToken.Token)}, 
                {nameof(RefreshToken.TokenFamily)},
                {nameof(RefreshToken.InvalidAt)}
                )
            VALUES (
                ${nameof(RefreshToken.UserId)}, 
                ${nameof(RefreshToken.Token)}, 
                ${nameof(RefreshToken.TokenFamily)}, 
                ${nameof(RefreshToken.InvalidAt)}
            );
            """, hashed);
        }
        catch (SqliteException ex) when (ex.SqliteExtendedErrorCode == SqliteError.ForeignKey_e)
        {
            _logger.LogInformation("Tried to add refresh token for user {id} who does not exist.", token.UserId);
            return Error.NotFound("User does not exist.");
        }
        return Result.Ok;
    }


    public async Task<RefreshTokenValidationResult> IsValid(Guid userId, string refreshToken)
    {
        // Get hash
        string hash = ComputeSHA256Hash(refreshToken);

        // Retrieve token
        using var connection = await GetConnection();
        var token = await connection.QueryFirstOrDefaultAsync<RefreshToken>($"""
            SELECT * FROM RefreshTokens
            WHERE {nameof(RefreshToken.UserId)} = @{nameof(userId)}
            AND {nameof(RefreshToken.Token)} = @{nameof(hash)}
            LIMIT 1;
            """, new { userId, hash });

        if (token is null)
        {
            _logger.LogInformation("Token with hash {hashStart}*** not found for user {id}.",
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
            _logger.LogWarning("Token with hash {hashStart}*** is invalidated for user {id}, revoking.",
                SafeSlicer.Slice(hash, 10), userId);

            await RevokeFamily(token.TokenFamily);
            return RefreshTokenValidationResult.Invalid;
        }

        // Invalidate token after use
        await connection.ExecuteAsync($"""
            UPDATE RefreshTokens
            SET {nameof(RefreshToken.InvalidatedAt)} = DATETIME('now')
            WHERE {nameof(RefreshToken.UserId)} = @{nameof(userId)}
            AND {nameof(RefreshToken.Token)} = @{nameof(hash)};
            """, new { userId, hash });

        return new RefreshTokenValidationResult(token.TokenFamily);
    }

    public async Task<int> RevokeUserTokens(Guid userId)
    {
        _logger.LogInformation("Revoking all tokens for user {id}.", userId);

        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE RefreshTokens
            SET {nameof(RefreshToken.InvalidatedAt)} = DATETIME('now')
            WHERE {nameof(RefreshToken.UserId)} = @{nameof(userId)};
            """, new { userId });
    }

    public async Task<int> RevokeFamily(Guid tokenFamilyId)
    {
        _logger.LogInformation("Revoking all tokens in token family {id}.", tokenFamilyId);

        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            UPDATE RefreshTokens
            SET {nameof(RefreshToken.InvalidatedAt)} = DATETIME('now')
            WHERE {nameof(RefreshToken.TokenFamily)} = @{nameof(tokenFamilyId)};
            """, new { tokenFamilyId });
    }

    public async Task<int> ClearOldEntries()
    {
        using var connection = await GetConnection();
        int rowsAffected = await connection.ExecuteAsync($"""
            DELETE FROM RefreshTokens
            WHERE {nameof(RefreshToken.InvalidAt)} < DATETIME('now');
            """);

        _logger.LogInformation("Cleared {count} expired refresh token entries.", rowsAffected);
        return rowsAffected;
    }

    protected override async Task InitializeAsync(SqliteConnection connection)
    {
        await connection.ExecuteAsync($"""
            CREATE TABLE IF NOT EXISTS RefreshTokens (
                {nameof(RefreshToken.UserId)} TEXT,
                {nameof(RefreshToken.Token)} TEXT NOT NULL,
                {nameof(RefreshToken.TokenFamily)} TEXT NOT NULL,
                {nameof(RefreshToken.InvalidAt)} DATETIME NOT NULL,
                {nameof(RefreshToken.InvalidatedAt)} DATETIME,
                {nameof(RefreshToken.CreatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(RefreshToken.UserId)}, {nameof(RefreshToken.Token)}),
                FOREIGN KEY ({nameof(RefreshToken.UserId)}) REFERENCES Users({nameof(User.Id)})
            );
            """);
    }

    private static string ComputeSHA256Hash(string refreshToken)
    {
        Guard.ThrowIfNull(refreshToken);

        byte[] tokenBytes = Encoding.UTF8.GetBytes(refreshToken);
        byte[] hash = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hash);
    }
}
