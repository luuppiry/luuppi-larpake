namespace LarpakeServer.Identity;

public class TokenDto
{
    public TokenDto() {}
    internal TokenDto(DateTime refreshExpiration)
    {
        RefreshExpiresAt = refreshExpiration;
    }


    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime? RefreshExpiresAt { get; } = null;
}
