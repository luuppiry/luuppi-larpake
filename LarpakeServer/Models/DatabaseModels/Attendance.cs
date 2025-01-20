namespace LarpakeServer.Models.DatabaseModels;

public class Attendance
{
    public required Guid UserId { get; set; }
    public required long EventId { get; set; }
    public Guid? CompletionId { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public AttendanceCompletion? Completion { get; set; } = null;

    public static Attendance MapFrom(long eventId, Guid userId)
    {
        return new Attendance
        {
            UserId = userId,
            EventId = eventId,
        };
    }
}
