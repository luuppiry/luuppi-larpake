using LarpakeServer.Data.TypeHandlers;

namespace LarpakeServer.Models.DatabaseModels;

public class Attendance
{
    public required Guid UserId { get; set; }

    [SqlColumn("larpake_event_id")]
    public required long LarpakeTaskId { get; set; }
    public Guid? CompletionId { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? QrCodeKey { get; set; } = null;
    public DateTime? KeyInvalidAt { get; set; } = null;

    public Completion? Completion { get; set; } = null;

    public static Attendance From(long eventId, Guid userId)
    {
        return new Attendance
        {
            UserId = userId,
            LarpakeTaskId = eventId,
        };
    }
}
