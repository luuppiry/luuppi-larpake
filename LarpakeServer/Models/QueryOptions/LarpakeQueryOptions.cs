using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.QueryOptions;

public class LarpakeQueryOptions : QueryOptions
{
    /* Title search is limited to admins only */

    public int? Year { get; set; } = null;
    
    [MinLength(3)]
    [MaxLength(30)]
    public string? Title { get; set; } = null;
    public Guid? ContainsUser { get; set; } = null;
    public bool DoMinimize { get; set; } = true;


    [JsonIgnore]
    public string? TitleQueryValue 
        => Title is null ? null : $"%{Title}%";
}
