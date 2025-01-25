using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.Templates;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos.MultipleItems;

public class LarpakeSectionsGetDto : GetDtoBase
{
    public required LarpakeSection[] Sections { get; set; }

    [JsonIgnore]
    public override int ItemCount => Sections.Length;
}
