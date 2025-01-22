
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;

namespace LarpakeServer.Models.DatabaseModels;

public class LarpakeEvent
{
    public required long Id { get; set; } 
    public required long LarpakeSectionId { get; set; } 
    public required string Title { get; set; }
    public required int Points { get; set; }
    public int OrderingWeightNumber { get; set; }
    public string? Body { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Guid>? ReferencedOrganizationEventIds { get; set; }

    internal static LarpakeEvent From(LarpakeEventPostDto record)
    {
        return new LarpakeEvent
        {
            Id = Constants.NullId,
            LarpakeSectionId = record.LarpakeSectionId,
            Title = record.Title,
            Points = record.Points,
            OrderingWeightNumber = record.OrderingWeightNumber,
            Body = record.Body,
            CancelledAt = null
        };
    }

    internal static LarpakeEvent From(LarpakeEventPutDto record)
    {
        return new LarpakeEvent
        {
            Id = Constants.NullId,
            LarpakeSectionId = Constants.NullId,
            Title = record.Title,
            Points = record.Points,
            OrderingWeightNumber = record.OrderingWeightNumber,
            Body = record.Body,
        };
    }
}
