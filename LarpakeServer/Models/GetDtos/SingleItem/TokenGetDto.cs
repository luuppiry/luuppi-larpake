namespace LarpakeServer.Models.GetDtos.SingleItem;

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
