using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;

namespace LarpakeServer.Models.DatabaseModels;

public class OrganizationEvent
{
    public required long Id { get; set; }
    public required string Title { get; set; }
    public string Body { get; set; } = string.Empty;
    public required DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; } = null;
    public string Location { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; } = null;
    public string? ImageUrl { get; set; } = null;
    
    // Metadata
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; } = null;
    public bool IsDeleted => DeletedAt is not null;


    public static OrganizationEvent MapFrom(EventPutDto dto, long id, Guid modifyingUser)
    {
        return new OrganizationEvent
        {
            Id = id,
            Title = dto.Title,
            Body = dto.Body,
            StartsAt = dto.StartsAt,
            EndsAt = dto.EndsAt,
            Location = dto.Location,
            WebsiteUrl = dto.WebsiteUrl,
            // TODO: ImageUrl = dto.ImageUrl,
            UpdatedBy = modifyingUser,
            DeletedAt = null

        };
    }

    public static OrganizationEvent MapFrom(EventPostDto dto, Guid modifyingUser)
    {
        return new OrganizationEvent
        {
            Id = Constants.NullId,
            Title = dto.Title,
            Body = dto.Body,
            StartsAt = dto.StartsAt,
            EndsAt = dto.EndsAt,
            Location = dto.Location,
            WebsiteUrl = dto.WebsiteUrl,
            // TODO: ImageUrl = dto.Image?.Url,
            CreatedBy = modifyingUser,
            UpdatedBy = modifyingUser,
            DeletedAt = null
        };
    }
}
