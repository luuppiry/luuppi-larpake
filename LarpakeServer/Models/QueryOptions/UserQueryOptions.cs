namespace LarpakeServer.Models.QueryOptions;

public class UserQueryOptions : QueryOptions
{
    public int? StartedBefore { get; set; } = null;
    
    public int? StartedAfter { get; set; } = null;

    public int? Permissions { get; set; } = null;

    public override bool HasNonNullValues()
    {
        return Permissions is not null
            || StartedBefore is not null
            || StartedAfter is not null;
    }
}
