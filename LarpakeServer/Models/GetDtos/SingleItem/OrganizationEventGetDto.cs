using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.Localizations;

namespace LarpakeServer.Models.GetDtos.SingleItem;

public class OrganizationEventGetDto
{
    public required long Id { get; set; }
    public required DateTime StartTimeUtc { get; set; }
    public DateTime? EndTimeUtc { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public SoftDeletionInfo? CancellationInfo { get; set; } = null;
    public required List<OrganizationEventLocalization> TextData { get; set; }

    internal static OrganizationEventGetDto From(OrganizationEvent record)
    {
        return new OrganizationEventGetDto
        {
            Id = record.Id,
            TextData = record.TextData.ToList(),
            StartTimeUtc = record.StartsAt,
            EndTimeUtc = record.EndsAt,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,
            CancellationInfo = record.CancelledAt.HasValue
                ? new SoftDeletionInfo(record.CancelledAt.Value) : null
        };
    }
}
