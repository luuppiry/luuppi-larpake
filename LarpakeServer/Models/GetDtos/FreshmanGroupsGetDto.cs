
using LarpakeServer.Models.DatabaseModels;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos;

public class FreshmanGroupsGetDto : IPageable
{
    public required FreshmanGroupGetDto[] Groups { get; set; }
    public int NextPage { get; set; } = -1;

    [JsonIgnore]
    public int ItemCount => Groups.Length;

    internal static FreshmanGroupsGetDto MapFrom(FreshmanGroup[] records)
    {
        return new FreshmanGroupsGetDto
        {
            Groups = records.Select(FreshmanGroupGetDto.From).ToArray(),
            NextPage = -1
        };
    }
}
