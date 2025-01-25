using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos.SingleItem;

public class LarpakeGetDto : Larpake
{
    public static LarpakeGetDto From(Larpake larpake)
    {
        return (LarpakeGetDto)larpake;
    }
}
