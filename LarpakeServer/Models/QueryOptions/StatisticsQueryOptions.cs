using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.QueryOptions;

public class StatisticsQueryOptions : QueryOptions
{
    [Required]
    public required long LarpakeId { get; set; }
}
