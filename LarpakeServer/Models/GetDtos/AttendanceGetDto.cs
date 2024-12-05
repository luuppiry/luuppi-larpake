namespace LarpakeServer.Models.GetDtos;

public class AttendanceGetDto
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required long EventId { get; set; }
    public AttendanceCompletedGetDto? Completed { get; set; } = null;
}
