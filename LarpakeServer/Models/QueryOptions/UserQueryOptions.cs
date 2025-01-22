namespace LarpakeServer.Models.QueryOptions;

public class UserQueryOptions : QueryOptions
{
    public int? StartedBefore { get; set; } = null;
    
    public int? StartedAfter { get; set; } = null;

    public int? Permissions { get; set; } = null;

 
}
