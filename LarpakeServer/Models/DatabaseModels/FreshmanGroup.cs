using LarpakeServer.Models.PostDtos;

namespace LarpakeServer.Models.DatabaseModels;

public class FreshmanGroup
{
    public required long Id { get; set; }
    public long LarpakeId { get; set; } = Constants.NullId;
    public required string Name { get; set; }
    public int? GroupNumber { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Guid>? Members { get; set; }

    internal static FreshmanGroup MapFrom(FreshmanGroupPostDto dto)
    {
        return new FreshmanGroup
        {
            Id = Constants.NullId,
            LarpakeId = dto.LarpakeId,
            Name = dto.Name,
            GroupNumber = dto.GroupNumber

        };
    }
}
