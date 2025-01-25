using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.SingleItem;
using LarpakeServer.Models.GetDtos.Templates;

namespace LarpakeServer.Models.GetDtos.MultipleItems;

public class LarpakkeetGetDto : GetDtoBase
{
    public required LarpakeGetDto[] Larpakkeet { get; set; }
    public override int ItemCount => Larpakkeet.Length;

    internal static LarpakkeetGetDto MapFrom(Larpake[] events)
    {
        return new LarpakkeetGetDto
        {
            Larpakkeet = events.Select(LarpakeGetDto.From).ToArray(),
        };
    }



}
