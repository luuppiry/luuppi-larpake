namespace LarpakeServer.Models.QueryOptions;

public class LarpakeEventQueryOptions : QueryOptions
{
    public required long? SectionId { get; set; }

    public required long? LarpakeId { get; set; }

    public bool? IsCancelled { get; set; } = null;
}
