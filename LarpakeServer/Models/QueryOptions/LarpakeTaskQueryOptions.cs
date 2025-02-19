namespace LarpakeServer.Models.QueryOptions;

public class LarpakeTaskQueryOptions : QueryOptions
{
    public Guid? UserId { get; set; }
    public long? GroupId { get; set; }
    public long? SectionId { get; set; }
    public long? LarpakeId { get; set; }
    public bool? IsCancelled { get; set; } = null;
    public long[]? LarpakeTaskIds { get; set; } = [];
}
