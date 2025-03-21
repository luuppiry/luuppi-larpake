using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.Templates;
using LarpakeServer.Services;

namespace LarpakeServer.Models.GetDtos;

public class AttendanceGetDto : IMappable<Attendance, AttendanceGetDto>
{
    public required Guid UserId { get; set; }
    public required long LarpakeTaskId { get; set; }
    public CompletionGetDto? Completed { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public AttendanceKey? Key { get; set; }

    public static AttendanceGetDto From(Attendance attendance)
    {
        return new AttendanceGetDto
        {
            UserId = attendance.UserId,
            LarpakeTaskId = attendance.LarpakeTaskId,
            Completed = attendance.Completion is null ?
                null : CompletionGetDto.From(attendance.Completion),
            CreatedAt = attendance.CreatedAt,
            UpdatedAt = attendance.UpdatedAt,
            Key = attendance.QrCodeKey is null ?
                null : new AttendanceKey(attendance.QrCodeKey, attendance.KeyInvalidAt ?? DateTime.UtcNow)

        };
    }
}
