namespace LarpakeServer.Models.QueryOptions;

public class UserQueryOptions : QueryOptions
{
    public int? StartedOnOrBefore { get; set; } = null;
    
    public int? StartedOnOrAfter { get; set; } = null;

    public int? Permissions { get; set; } = null;

    public override bool HasNonNullValues()
    {
        return Permissions is not null
            || StartedOnOrBefore is not null
            || StartedOnOrAfter is not null;
    }
}
