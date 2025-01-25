using System.Text.Json.Serialization;

namespace LarpakeServer.Models.QueryOptions;

public class LarpakeQueryOptions : QueryOptions
{
    public int? Year { get; set; } = null;
    public string? Title { get; set; } = null;
    public Guid? ContainsUser { get; set; } = null;
    public bool DoMinimize { get; set; } = true;


    [JsonIgnore]
    public string? TitleQueryValue 
        => Title is null ? null : $"%{Title}%";
}
