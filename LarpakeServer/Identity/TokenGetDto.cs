namespace LarpakeServer.Identity;

public class TokenGetDto
{
    public TokenGetDto() {}
    internal TokenGetDto(DateTime refreshExpiration)
    {
        RefreshExpiresAt = refreshExpiration;
    }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime? RefreshExpiresAt { get; } = null;
}
