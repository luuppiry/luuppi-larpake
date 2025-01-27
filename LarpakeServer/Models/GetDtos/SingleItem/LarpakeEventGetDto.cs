using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos.SingleItem;

public class LarpakeEventGetDto
{
    public class Localization : ILocalization
    {
        public string Lang => Constants.LangDefault;
        public required string Title { get; set; }
        public string? Body { get; set; }

    }

    public required long Id { get; set; }
    public required Localization[] TextData { get; set; }
    public required long LarpakeSectionId { get; set; }
    public required int Points { get; set; }
    public int OrderingWeightNumber { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Guid>? ReferencedOrganizationEventIds { get; set; }



    internal static LarpakeEventGetDto From(LarpakeEvent record)
    {
        return new LarpakeEventGetDto
        {
            Id = record.Id,
            TextData = [new Localization
            {
                Title = record.Title,
                Body = record.Body
            }],
            LarpakeSectionId = record.LarpakeSectionId,
            Points = record.Points,
            OrderingWeightNumber = record.OrderingWeightNumber,
            CancelledAt = record.CancelledAt,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,
            ReferencedOrganizationEventIds = record.ReferencedOrganizationEventIds
        };
    }
}
