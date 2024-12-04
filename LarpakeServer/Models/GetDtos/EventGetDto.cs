namespace LarpakeServer.Models.GetDtos;

public class EventGetDto
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string Body { get; set; } = string.Empty;
    public required DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; } = null;
    public required string Location { get; set; }
    public string? WebsiteUrl { get; set; } = null;
    public string? ImageUrl { get; set; } = null;
    public SoftDeletionInfo? SoftDeletionInfo { get; set; } = null;
}
