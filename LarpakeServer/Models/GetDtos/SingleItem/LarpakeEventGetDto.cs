using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.Localizations;

namespace LarpakeServer.Models.GetDtos.SingleItem;

public class LarpakeEventGetDto
{
    public required long Id { get; set; }
    public required long LarpakeSectionId { get; set; }
    public required int Points { get; set; }
    public int OrderingWeightNumber { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public required List<LarpakeEventLocalization> TextData { get; set; }



    internal static LarpakeEventGetDto From(LarpakeEvent record)
    {
        return new LarpakeEventGetDto
        {
            Id = record.Id,
            TextData = record.TextData,
            LarpakeSectionId = record.LarpakeSectionId,
            Points = record.Points,
            OrderingWeightNumber = record.OrderingWeightNumber,
            CancelledAt = record.CancelledAt,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,
        };
    }
}
