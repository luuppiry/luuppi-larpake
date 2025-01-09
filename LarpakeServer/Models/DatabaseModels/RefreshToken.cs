namespace LarpakeServer.Models.DatabaseModels;

public class RefreshToken
{
    public required Guid UserId { get; set; }
    public required string Token { get; set; }
    public required DateTime InvalidAt { get; set; }
    public DateTime? InvalidatedAt { get; set; } = null;
    public Guid TokenFamily { get; set; } = Guid.Empty;
    public DateTime CreatedAt { get; set; }
}
