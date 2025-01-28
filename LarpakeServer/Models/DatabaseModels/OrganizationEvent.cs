using LarpakeServer.Models.DatabaseModels.Metadata;
using LarpakeServer.Models.Localizations;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;

namespace LarpakeServer.Models.DatabaseModels;

public class OrganizationEvent : ILocalized<OrganizationEventLocalization>
{
    public required long Id { get; set; }

    public required DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; } = null;
    public required string Location { get; set; }

    // Metadata
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; } = null;
    public bool IsDeleted => CancelledAt is not null;
    public required List<OrganizationEventLocalization> TextData { get; set; }
    internal OrganizationEventLocalization DefaultLocalization => GetDefaultLocalization();


    public static OrganizationEvent MapFrom(OrganizationEventPutDto dto, long id, Guid modifyingUser)
    {
        return new OrganizationEvent
        {
            Id = id,
            Location = dto.Location,
            TextData = dto.TextData.ToList(),
            StartsAt = dto.StartsAt,
            EndsAt = dto.EndsAt,
            UpdatedBy = modifyingUser,
        };
    }

    public static OrganizationEvent MapFrom(OrganizationEventPostDto dto, Guid modifyingUser)
    {
        return new OrganizationEvent
        {
            Id = Constants.NullId,
            Location = dto.Location,
            TextData = dto.TextData.ToList(),
            StartsAt = dto.StartsAt,
            EndsAt = dto.EndsAt,
            CreatedBy = modifyingUser,
            UpdatedBy = modifyingUser,
        };
    }

    private OrganizationEventLocalization GetDefaultLocalization()
    {
        if (TextData is null || TextData.Count is 0)
        {
            throw new InvalidOperationException("No localization data found.");
        }
        return TextData.FirstOrDefault(
            x => x.LanguageCode == Constants.LangDefault, TextData.First());
    }
}
