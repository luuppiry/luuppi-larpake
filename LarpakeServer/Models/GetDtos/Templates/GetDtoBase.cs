
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos.Templates;

public abstract class GetDtoBase : IDetails, IPageable
{
    [JsonIgnore]
    public abstract int ItemCount { get; }
    public int NextPage { get; set; } = -1;
    public List<string> Details { get; } = [];
    public List<ApiAction> Actions { get; } = [];
}
