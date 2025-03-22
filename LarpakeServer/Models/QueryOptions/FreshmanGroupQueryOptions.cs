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
    public long? LarpakeId { get; set; }
    public int? GroupNumber { get; set; }
    public bool DoMinimize { get; set; } = true;
    public bool? IsSearchMemberCompeting { get; set; }
    public bool IncludeHiddenMembers { get; set; } = true;
    public bool IsORQuery { get; set; } = false;

    [JsonIgnore]
    public string? GroupNameSearchValue => GroupName is null ? null : $"%{GroupName}%";
}
