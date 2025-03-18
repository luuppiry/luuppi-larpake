namespace LarpakeServer.Models.QueryOptions;

public class AttendanceQueryOptions : QueryOptions
{
    public DateTime? Before { get; set; } = null;
    public DateTime? CompletedBefore { get; set; } = null;
    public DateTime? After { get; set; } = null;
    public DateTime? CompletedAfter { get; set; } = null;
    public long? LarpakeId { get; set; } = null;
    public long? LarpakeTaskId { get; set; } = null;
    public Guid? UserId { get; set; } = null;
    public bool? IsCompleted { get; set; } = null;



}
