
using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos;

public class FreshmanGroupsGetDto
{
    public required FreshmanGroupGetDto[] Groups { get; set; }
    public int NextPage { get; set; } = -1;

    internal static FreshmanGroupsGetDto MapFrom(FreshmanGroup[] records)
    {
        return new FreshmanGroupsGetDto
        {
            Groups = records.Select(FreshmanGroupGetDto.From).ToArray(),
            NextPage = -1
        };
    }
}
