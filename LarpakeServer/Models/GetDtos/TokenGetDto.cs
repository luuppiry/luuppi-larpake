namespace LarpakeServer.Models.GetDtos;

public class TokenGetDto
{
    public TokenGetDto() { }
    internal TokenGetDto(DateTime accessExpiration, DateTime refreshExpiration)
    {
        AccessTokenExpiresAt = accessExpiration;
        RefreshTokenExpiresAt = refreshExpiration;
    }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }

    public DateTime? AccessTokenExpiresAt { get; } = null;
    public DateTime? RefreshTokenExpiresAt { get; } = null;
}
