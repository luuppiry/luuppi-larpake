namespace LarpakeServer.Models.DatabaseModels;

public class RefreshToken
{
    string? _token;

    public required Guid UserId { get; set; }
    public required string Token { get; set; }
    public required DateTime InvalidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
