namespace LarpakeServer.Models.QueryOptions;

public class LarpakeEventQueryOptions : QueryOptions
{
    public Guid? UserId { get; set; }
    public long? GroupId { get; set; }
    public required long? SectionId { get; set; }
    public required long? LarpakeId { get; set; }
    public bool? IsCancelled { get; set; } = null;
}
