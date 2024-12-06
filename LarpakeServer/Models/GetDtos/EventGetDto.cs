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
    public DateTime CreatedUtc { get; set; }
    public DateTime LastModifiedUtc { get; set; }
    public SoftDeletionInfo? SoftDeletionInfo { get; set; } = null;

    internal static EventGetDto From(Event record)
    {
        return new EventGetDto
        {
            Id = record.Id,
            Title = record.Title,
            Body = record.Body,
            StartTimeUtc = record.StartTimeUtc,
            EndTimeUtc = record.EndTimeUtc,
            Location = record.Location,
            WebsiteUrl = record.WebsiteUrl,
            ImageUrl = record.ImageUrl,
            CreatedUtc = record.CreatedUtc,
            LastModifiedUtc = record.LastModifiedUtc,

            SoftDeletionInfo = record.TimeDeletedUtc.HasValue
                ? new SoftDeletionInfo(record.TimeDeletedUtc.Value) : null
        };
    }
}
