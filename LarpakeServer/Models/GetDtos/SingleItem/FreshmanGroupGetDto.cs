using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos.SingleItem;

public class FreshmanGroupGetDto : FreshmanGroup
{
    internal static FreshmanGroupGetDto From(FreshmanGroup group)
    {
        return new FreshmanGroupGetDto
        {
            Id = group.Id,
            LarpakeId = group.LarpakeId,
            Name = group.Name,
            GroupNumber = group.GroupNumber,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt,
            Members = group.Members
        };
    }
}
