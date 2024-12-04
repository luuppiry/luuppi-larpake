namespace LarpakeServer.Models.DatabaseModels;

public class Event
{
    public required long Id { get; set; }
    public required string Title { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public string Location { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public DateTime? TimeDeleted { get; set; } = null;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime LastModifiedUtc { get; set; }
}
