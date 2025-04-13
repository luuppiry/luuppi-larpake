using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Services;

namespace LarpakeServer.Models.GetDtos;

public class FatAttendanceGetDto : AttendanceGetDto
{
    public required UserGetDto User { get; set; }
    public required LarpakeTaskGetDto Task { get; set; }

    public static FatAttendanceGetDto From(Attendance attendance, UserGetDto user, LarpakeTask task)
    {
        LarpakeTaskGetDto taskDto = LarpakeTaskGetDto.From(task);
        user.EntraId = null;
        user.EntraUsername = null;
        user.StartYear = null;

        return new FatAttendanceGetDto
        {
            UserId = attendance.UserId,
            LarpakeTaskId = attendance.LarpakeTaskId,
            Completed = attendance.Completion is null ?
                null : CompletionGetDto.From(attendance.Completion),
            CreatedAt = attendance.CreatedAt,
            UpdatedAt = attendance.UpdatedAt,
            Key = attendance.QrCodeKey is null ?
                null : new AttendanceKey(attendance.QrCodeKey, attendance.KeyInvalidAt ?? DateTime.UtcNow),
            User = user,
            Task = taskDto
        };
    }
}
