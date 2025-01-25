using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Services;

namespace LarpakeServer.Models.GetDtos.SingleItem;

public class AttendanceGetDto
{
    public required Guid UserId { get; set; }
    public required long LarpakeEventId { get; set; }
    public AttendanceCompletionGetDto? Completed { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public AttendanceKey? Key { get; set; }

    internal static AttendanceGetDto From(Attendance attendance)
    {
        return new AttendanceGetDto
        {
            UserId = attendance.UserId,
            LarpakeEventId = attendance.LarpakeEventId,
            Completed = attendance.Completion is null ?
                null : AttendanceCompletionGetDto.From(attendance.Completion),
            CreatedAt = attendance.CreatedAt,
            UpdatedAt = attendance.UpdatedAt,
            Key = attendance.QrCodeKey is null ?
                null : new AttendanceKey(attendance.QrCodeKey, attendance.KeyInvalidAt ?? DateTime.UtcNow)

        };
    }
}
