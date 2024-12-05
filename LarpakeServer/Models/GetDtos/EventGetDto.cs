using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos;

public class EventGetDto
{
    public required long Id { get; set; }
    public required string Title { get; set; }
    public string Body { get; set; } = string.Empty;
    public required DateTime StartTimeUtc { get; set; }
    public DateTime? EndTimeUtc { get; set; } = null;
    public required string Location { get; set; }
    public string? WebsiteUrl { get; set; } = null;
    public string? ImageUrl { get; set; } = null;
    public SoftDeletionInfo? SoftDeletionInfo { get; set; } = null;

    internal static object? From(Event event_)
    {
        throw new NotImplementedException();
    }
}
