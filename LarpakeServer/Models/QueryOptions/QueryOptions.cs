using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.QueryOptions;

public class QueryOptions
{
    [Range(1, int.MaxValue)]
    public virtual int PageSize { get; set; } = 20;

    [Range(0, int.MaxValue)]
    public virtual int PageOffset { get; set; } = 0;
    
    public virtual int GetNextOffset()
    {
        return PageOffset + PageSize;
    }
}
