using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos.SingleItem;

public class FreshmanGroupGetDto : FreshmanGroup
{
    internal static FreshmanGroupGetDto From(FreshmanGroup group)
    {
        return (FreshmanGroupGetDto)group;
    }
}
