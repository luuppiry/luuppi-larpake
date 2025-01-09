using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.QueryOptions;

public class FreshmanGroupQueryOptions : QueryOptions
{
    [MaxLength(30)]
    [MinLength(3)]
    public string? GroupName { get; set; }
    public Guid? ContainsUser { get; set; }
    public int? StartYear { get; set; }
    public bool DoMinimize { get; set; } = true;

    [JsonIgnore]
    public bool IncludeHiddenMembers { get; set; } = false;


    public override bool HasNonNullValues()
    {
        return GroupName is not null
            || ContainsUser is not null
            || StartYear is not null;
    }
}
