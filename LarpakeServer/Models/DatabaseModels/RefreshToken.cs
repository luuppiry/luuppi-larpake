namespace LarpakeServer.Models.DatabaseModels;

public class RefreshToken
{
    public required Guid UserId { get; set; }
    public required string Token { get; set; }
    public required DateTime InvalidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
