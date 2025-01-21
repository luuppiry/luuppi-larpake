using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos;

public class AttendanceGetDto
{
    public required Guid UserId { get; set; }
    public required long EventId { get; set; }
    public AttendanceCompletionGetDto? Completed { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    internal static AttendanceGetDto From(Attendance attendance)
    {
        return new AttendanceGetDto
        {
            UserId = attendance.UserId,
            EventId = attendance.LarpakeEventId,
            Completed = attendance.Completion is null ?
                null : AttendanceCompletionGetDto.From(attendance.Completion)
        };
    }
}
