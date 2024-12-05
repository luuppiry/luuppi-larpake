namespace LarpakeServer.Models.DatabaseModels;

public class EventAttendance
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required long EventId { get; set; }
    public Guid? CreationClientId { get; set; }
    public DateTime CreatedUtc { get; set; }
}
