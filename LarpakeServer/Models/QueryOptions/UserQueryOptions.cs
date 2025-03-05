using System.Text.Json.Serialization;

namespace LarpakeServer.Models.QueryOptions;

public class UserQueryOptions : QueryOptions
{
    public int? StartedBefore { get; set; } = null;

    public int? StartedAfter { get; set; } = null;

    public int? Permissions { get; set; } = null;
    
    public Guid[]? UserIds { get; set; }
    
    public Guid[]? EntraIds { get; set; }

    public string? EntraUsername { get; set; }

    [JsonIgnore]
    public string? EntraUsernameQueryValue => EntraUsername is null ? null : $"%{EntraUsername}%";
}
