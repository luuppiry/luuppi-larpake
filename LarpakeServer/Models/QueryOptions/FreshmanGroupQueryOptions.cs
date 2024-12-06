using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.QueryOptions;

public class FreshmanGroupQueryOptions : QueryOptions
{
    [MaxLength(30)]
    [MinLength(3)]
    public string? Name { get; set; }
    public Guid? ContainsUser { get; set; }
    public int? SizeMin { get; set; }
    public int? SizeMax { get; set; }
    public int? StartYear { get; set; }
    public bool DoMinimize { get; set; } = true;


    public override bool HasNonNullValues()
    {
        return Name is not null
            || ContainsUser is not null
            || SizeMin is not null
            || SizeMax is not null
            || StartYear is not null;
    }
}
