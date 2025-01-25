using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;

namespace LarpakeServer.Models.DatabaseModels;

public class LarpakeSection
{
    public required long Id { get; set; }
    public required long LarpakeId { get; set; }
    public required string Title { get; set; }
    public int OrderingWeightNumber { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    internal static LarpakeSection From(LarpakeSectionPostDto dto, long larpakeId)
    {
        return new LarpakeSection
        {
            Id = Constants.NullId,
            LarpakeId = larpakeId,
            Title = dto.Title,
            OrderingWeightNumber = dto.OrderingWeightNumber,
        };
    }
    
    internal static LarpakeSection From(LarpakeSectionPutDto dto, long larpakeId)
    {
        var section = From((LarpakeSectionPostDto)dto, larpakeId);
        section.Id = dto.Id;
        return section;
    }
}
