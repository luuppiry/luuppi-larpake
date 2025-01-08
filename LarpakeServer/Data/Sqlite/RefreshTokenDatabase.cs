using LarpakeServer.Models.DatabaseModels;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;

namespace LarpakeServer.Data.Sqlite;

public class RefreshTokenDatabase : SqliteDbBase, IRefreshTokenDatabase
{
    readonly IConfiguration _configuration;

    public RefreshTokenDatabase(
        SqliteConnectionString connectionString,
        IConfiguration configuration,
        UserDatabase userDb) : base(connectionString, userDb)
    {
        Guard.ThrowIfNull(configuration["Jwt:RefreshTokenSalt"]);
        _configuration = configuration;
    }



    public async Task<Result<bool>> Add(RefreshToken token)
    {
        if (token.UserId == Guid.Empty)
        {
            return Error.BadRequest("User id cannot be empty.");
        }
        if (token.Token is null)
        {
            return Error.BadRequest("Refresh token cannot be null.");
        }

        // Hash the refresh token
        var hashed = new RefreshToken
        {
            UserId = token.UserId,
            Token = ComputeSHA512Hash(token.Token),
            InvalidAt = token.InvalidAt
        };

        using var connection = await GetConnection();
        try
        {
            await connection.ExecuteAsync($"""
            INSERT INTO RefreshTokens (
                {nameof(RefreshToken.UserId)}, 
                {nameof(RefreshToken.Token)}, 
                {nameof(RefreshToken.InvalidAt)})
            VALUES (
                ${nameof(RefreshToken.UserId)}, 
                ${nameof(RefreshToken.Token)}, 
                ${nameof(RefreshToken.InvalidAt)}
            );
            """, hashed);
        }
        catch (SqliteException ex) when (ex.SqliteExtendedErrorCode == SqliteError.ForeignKey_e)
        {
            return Error.NotFound("User does not exist.");
        }
        catch (SqliteException ex) when (ex.SqliteExtendedErrorCode == SqliteError.ForeignKey_e)
        {
            // Already exists
        }
        return true;
    }


    public async Task<bool> IsValid(Guid userId, string refreshToken)
    {
        string hash = ComputeSHA512Hash(refreshToken);

        using var connection = await GetConnection();
        var token = await connection.QueryFirstOrDefaultAsync<RefreshToken>($"""
            DELETE FROM RefreshTokens
            WHERE 
                {nameof(RefreshToken.UserId)} = @{nameof(userId)} 
                AND {nameof(RefreshToken.Token)} = @{nameof(hash)}
                AND {nameof(RefreshToken.InvalidAt)} > DATETIME('now')
            RETURNING *;
            """, new { userId, hash });
        return token is not null;
    }

    public async Task<int> Delete(Guid userId)
    {
        using var connection = await GetConnection();
        return await connection.ExecuteAsync($"""
            DELETE FROM RefreshTokens
            WHERE {nameof(RefreshToken.UserId)} = @{nameof(userId)};
            """, new { userId });
    }

    public async Task ClearOldEntries()
    {
        using var connection = await GetConnection();
        await connection.ExecuteAsync($"""
            DELETE FROM RefreshTokens
            WHERE {nameof(RefreshToken.InvalidAt)} < DATETIME('now');
            """);
    }

    protected override async Task InitializeAsync(SqliteConnection connection)
    {
        await connection.ExecuteAsync($"""
            CREATE TABLE IF NOT EXISTS RefreshTokens (
                {nameof(RefreshToken.UserId)} TEXT,
                {nameof(RefreshToken.Token)} TEXT NOT NULL,
                {nameof(RefreshToken.InvalidAt)} DATETIME NOT NULL,
                {nameof(RefreshToken.CreatedAt)} DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY ({nameof(RefreshToken.UserId)}, {nameof(RefreshToken.Token)}),
                FOREIGN KEY ({nameof(RefreshToken.UserId)}) REFERENCES Users({nameof(User.Id)})
            );
            """);
    }

    private string ComputeSHA512Hash(string refreshToken)
    {
        Guard.ThrowIfNull(refreshToken);

        string salt = _configuration["Jwt:RefreshTokenSalt"]!;
        byte[] tokenBytes = Encoding.UTF8.GetBytes(refreshToken + salt);
        byte[] hash = SHA512.HashData(tokenBytes);
        return Convert.ToHexString(hash);
    }
}
