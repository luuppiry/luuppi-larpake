using LarpakeServer.Models.PostDtos;

namespace LarpakeServer.Models.DatabaseModels;

public class FreshmanGroup
{
    public required long Id { get; set; }
    public required string Name { get; set; }
    public int? StartYear { get; set; } = null;
    public int? GroupNumber { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Guid>? Members { get; set; }

    internal static FreshmanGroup MapFrom(FreshmanGroupPostDto dto)
    {
        return new FreshmanGroup
        {
            Id = Constants.NullId,
            Name = dto.Name,
            StartYear = dto.StartYear,
            GroupNumber = dto.GroupNumber,
        };
    }
}
