namespace LarpakeServer.Models.DatabaseModels;

public class Attendance
{
    public required Guid UserId { get; set; }
    public required long LarpakeEventId { get; set; }
    public Guid? CompletionId { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? QrCodeKey { get; set; } = null;
    public DateTime? KeyInvalidAt { get; set; } = null;

    public Completion? Completion { get; set; } = null;

    public static Attendance MapFrom(long eventId, Guid userId)
    {
        return new Attendance
        {
            UserId = userId,
            LarpakeEventId = eventId,
        };
    }
}
