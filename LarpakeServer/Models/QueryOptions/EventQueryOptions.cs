using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.QueryOptions;

public class EventQueryOptions : QueryOptions
{
    public DateTime? BeforeUtc { get; set; } = null;
    public DateTime? AfterUtc { get; set; } = null;

    [MinLength(3)]
    [MaxLength(30)]
    public string? Title { get; set; } = null;
    public bool DoMinimize { get; set; } = false;

    public override bool HasNonNullValues()
    {
        return BeforeUtc is not null 
            || AfterUtc is not null 
            || Title is not null;
    }
}
