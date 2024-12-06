using LarpakeServer.Models.PostDtos;

namespace LarpakeServer.Models.DatabaseModels;

public class Event
{
    public required long Id { get; set; }
    public required string Title { get; set; }
    public string Body { get; set; } = string.Empty;
    public required DateTime StartTimeUtc { get; set; }
    public DateTime? EndTimeUtc { get; set; } = null;
    public string Location { get; set; } = string.Empty;
    public long LuuppiRefId { get; set; } = Constants.NullId;
    public string? WebsiteUrl { get; set; } = null;
    public string? ImageUrl { get; set; } = null;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedUtc { get; set; }
    public Guid LastModifiedBy { get; set; }
    public DateTime LastModifiedUtc { get; set; }
    public DateTime? TimeDeletedUtc { get; set; } = null;
    public bool IsDeleted => TimeDeletedUtc is not null;



    public static Event MapFrom(EventPostDto dto, Guid requestUserId)
    {
        return new Event
        {
            Id = Constants.NullId,
            Title = dto.Title,
            Body = dto.Body,
            StartTimeUtc = dto.StartTimeUtc,
            EndTimeUtc = dto.EndTimeUtc,
            Location = dto.Location,
            LuuppiRefId = dto.LuuppiRefId,
            WebsiteUrl = dto.WebsiteUrl,
            // TODO: Image ImageUrl = dto.Image?.Url,
            CreatedBy = requestUserId,
            LastModifiedBy = requestUserId,
            TimeDeletedUtc = null
        };
    }
}
