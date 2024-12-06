using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos;

public class FreshmanGroupGetDto
{
    public required long Id { get; set; }
    public required string Name { get; set; }
    public int? StartYear { get; set; } = null;
    public int? GroupNumber { get; set; } = null;
    public List<Guid>? MemberIds { get; set; } = [];

    internal static FreshmanGroupGetDto From(FreshmanGroup group)
    {
        return new FreshmanGroupGetDto
        {
            Id = group.Id,
            Name = group.Name,
            StartYear = group.StartYear,
            GroupNumber = group.GroupNumber,
            MemberIds = group.Members
        };
    }
}
