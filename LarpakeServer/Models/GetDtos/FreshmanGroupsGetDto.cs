
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.Templates;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos;

public class FreshmanGroupsGetDto : GetDtoBase
{
    public required FreshmanGroupGetDto[] Groups { get; set; }

    [JsonIgnore]
    public override int ItemCount => Groups.Length;

    internal static FreshmanGroupsGetDto MapFrom(FreshmanGroup[] records)
    {
        return new FreshmanGroupsGetDto
        {
            Groups = records.Select(FreshmanGroupGetDto.From).ToArray(),
            NextPage = -1
        };
    }
}
