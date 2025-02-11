using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.Templates;

namespace LarpakeServer.Models.GetDtos;

public class FreshmanGroupGetDto : FreshmanGroup, IMappable<FreshmanGroup, FreshmanGroupGetDto>
{
    public static FreshmanGroupGetDto From(FreshmanGroup group)
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
