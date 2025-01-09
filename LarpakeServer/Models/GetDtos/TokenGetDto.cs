namespace LarpakeServer.Models.GetDtos;

public class TokenGetDto
{
    public TokenGetDto() { }
    internal TokenGetDto(DateTime refreshExpiration)
    {
        RefreshTokenExpiresAt = refreshExpiration;
    }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime? RefreshTokenExpiresAt { get; } = null;
}
