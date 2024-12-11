using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos;

public interface IPageable
{
    abstract int NextPage { get; set; }

    [JsonIgnore]
    abstract int ItemCount { get; }
}
