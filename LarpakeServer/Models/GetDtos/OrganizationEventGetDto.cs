using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.Templates;
using LarpakeServer.Models.Localizations;

namespace LarpakeServer.Models.GetDtos;

public class OrganizationEventGetDto : IMappable<OrganizationEvent, OrganizationEventGetDto>
{
    public required long Id { get; set; }
    public required DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public SoftDeletionInfo? CancellationInfo { get; set; } = null;
    public required List<OrganizationEventLocalization> TextData { get; set; }

    public static OrganizationEventGetDto From(OrganizationEvent record)
    {
        return new OrganizationEventGetDto
        {
            Id = record.Id,
            TextData = record.TextData.ToList(),
            StartsAt = record.StartsAt,
            EndsAt = record.EndsAt,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,
            CancellationInfo = record.CancelledAt.HasValue
                ? new SoftDeletionInfo(record.CancelledAt.Value) : null
        };
    }
}
