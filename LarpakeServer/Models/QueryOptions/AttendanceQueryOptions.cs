namespace LarpakeServer.Models.QueryOptions;

public class AttendanceQueryOptions : QueryOptions
{
    public DateTime? BeforeUtc { get; set; } = null;
    public DateTime? AfterUtc { get; set; } = null;
    public long? EventId { get; set; } = null;
    public Guid? UserId { get; set; } = null;
    public bool? IsCompleted { get; set; } = null;


    public override bool HasNonNullValues()
    {
        return BeforeUtc is not null 
            || AfterUtc is not null 
            || EventId is not null 
            || UserId is not null 
            || IsCompleted is not null;
    }
}
