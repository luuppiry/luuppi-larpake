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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public SoftDeletionInfo? SoftDeletionInfo { get; set; } = null;

    internal static EventGetDto From(OrganizationEvent record)
    {
        return new EventGetDto
        {
            Id = record.Id,
            Title = record.Title,
            Body = record.Body,
            StartTimeUtc = record.StartsAt,
            EndTimeUtc = record.EndsAt,
            Location = record.Location,
            WebsiteUrl = record.WebsiteUrl,
            ImageUrl = record.ImageUrl,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,

            SoftDeletionInfo = record.DeletedAt.HasValue
                ? new SoftDeletionInfo(record.DeletedAt.Value) : null
        };
    }
}
