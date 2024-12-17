using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;

namespace LarpakeServer.Models.DatabaseModels;

public class Event
{
    public required long Id { get; set; }
    public required string Title { get; set; }
    public string Body { get; set; } = string.Empty;
    public required DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; } = null;
    public string Location { get; set; } = string.Empty;
    public long LuuppiRefId { get; set; } = Constants.NullId;
    public string? WebsiteUrl { get; set; } = null;
    public string? ImageUrl { get; set; } = null;
    
    // Metadata
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; } = null;
    public bool IsDeleted => DeletedAt is not null;


    public static Event MapFrom(EventPutDto dto, Guid requestUserId, long id)
    {
        return new Event
        {
            Id = id,
            Title = dto.Title,
            Body = dto.Body,
            StartsAt = dto.StartsAt,
            EndsAt = dto.EndsAt,
            Location = dto.Location,
            LuuppiRefId = dto.LuuppiRefId,
            WebsiteUrl = dto.WebsiteUrl,
            // TODO: ImageUrl = dto.ImageUrl,
            UpdatedBy = requestUserId,
            DeletedAt = null

        };
    }

    public static Event MapFrom(EventPostDto dto, Guid requestUserId)
    {
        return new Event
        {
            Id = Constants.NullId,
            Title = dto.Title,
            Body = dto.Body,
            StartsAt = dto.StartsAt,
            EndsAt = dto.EndsAt,
            Location = dto.Location,
            LuuppiRefId = dto.LuuppiRefId,
            WebsiteUrl = dto.WebsiteUrl,
            // TODO: ImageUrl = dto.Image?.Url,
            CreatedBy = requestUserId,
            UpdatedBy = requestUserId,
            DeletedAt = null
        };
    }
}
