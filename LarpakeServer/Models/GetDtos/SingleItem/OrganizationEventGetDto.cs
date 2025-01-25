using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos.SingleItem;

public class OrganizationEventGetDto
{
    public class LanguageData
    {
        public required string Language { get; set; }
        public required string Title { get; set; }
        public string Body { get; set; } = string.Empty;
        public string? WebsiteUrl { get; set; } = null;
        public string? ImageUrl { get; set; } = null;
    }

    public required long Id { get; set; }
    public required LanguageData[] TextData { get; set; }
    public required DateTime StartTimeUtc { get; set; }
    public DateTime? EndTimeUtc { get; set; } = null;
    public required string Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public SoftDeletionInfo? CancellationInfo { get; set; } = null;

    internal static OrganizationEventGetDto From(OrganizationEvent record)
    {
        return new OrganizationEventGetDto
        {
            Id = record.Id,
            TextData = [
                new LanguageData
                {
                    Language = Constants.LangDefault,
                    Title = record.Title,
                    Body = record.Body,
                    WebsiteUrl = record.WebsiteUrl,
                    ImageUrl = record.ImageUrl,
                }
            ],
            StartTimeUtc = record.StartsAt,
            EndTimeUtc = record.EndsAt,
            Location = record.Location,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,
            CancellationInfo = record.CancelledAt.HasValue
                ? new SoftDeletionInfo(record.CancelledAt.Value) : null
        };
    }
}
