using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.QueryOptions;

public class UserQueryOptions : QueryOptions
{
    /*  TODO: Add support for name search
    [Length(2, 50)]
    public string? FirstName { get; set; } = null;

    [Length(2, 50)]
    public string? LastName { get; set; } = null;

    [Length(5, 50)]
    public string? UserName { get; set; } = null;
    */

    public int? StartedOnOrBefore { get; set; } = null;
    
    public int? StartedOnOrAfter { get; set; } = null;

    public Permissions? Permissions { get; set; } = null;

    public override bool HasNonNullValues()
    {
        return Permissions is not null
            || StartedOnOrBefore is not null
            || StartedOnOrAfter is not null;
            /* TODO: Add support for name search
            || FirstName is not null
            || LastName is not null 
            || UserName is not null 
            */
    }
}
