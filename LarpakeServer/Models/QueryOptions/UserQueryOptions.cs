using System.Text.Json.Serialization;

namespace LarpakeServer.Models.QueryOptions;

public class UserQueryOptions : QueryOptions
{
    public int? StartedBefore { get; set; } = null;

    public int? StartedAfter { get; set; } = null;

    public int? Permissions { get; set; } = null;

    public bool IsORQuery { get; set; } = false;
    
    public Guid[]? UserIds { get; set; }
    
    public Guid[]? EntraIds { get; set; }

    public string? EntraUsername { get; set; }

    [JsonIgnore]
    public string? EntraUsernameQueryValue => EntraUsername is null ? null : $"%{EntraUsername}%";
}
