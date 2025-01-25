using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos.SingleItem;

public class LarpakeEventGetDto : LarpakeEvent
{
    internal static LarpakeEventGetDto From(LarpakeEvent record)
    {
        return new LarpakeEventGetDto
        {
            Id = record.Id,
            LarpakeSectionId = record.LarpakeSectionId,
            Title = record.Title,
            Points = record.Points,
            OrderingWeightNumber = record.OrderingWeightNumber,
            Body = record.Body,
            CancelledAt = record.CancelledAt,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,
            ReferencedOrganizationEventIds = record.ReferencedOrganizationEventIds
        };
    }
}
