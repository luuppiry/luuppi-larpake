using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.QueryOptions;

public class EventQueryOptions
{
    public DateTime? BeforeUtc { get; set; } = null;
    public DateTime? AfterUtc { get; set; } = null;

    [MinLength(3)]
    [MaxLength(30)]
    public string? Title { get; set; } = null;
    public bool DoMinimize { get; set; } = false;

    [Range(1, int.MaxValue)]
    public int PageSize { get; set; } = 20;

    [Range(0, int.MaxValue)]
    public int PageOffset { get; set; } = 0;

    public bool HasNonNullValues()
    {
        return BeforeUtc is not null || AfterUtc is not null || Title is not null;
    }

    public int GetNextOffset()
    {
        return PageOffset + PageSize;
    }
}
