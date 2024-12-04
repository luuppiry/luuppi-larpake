namespace LarpakeServer.Models.GetDtos;

public class AttendanceCompletedGetDto
{
    public required Guid Id { get; set; }
    public required Guid SignerId { get; set; }
    public Guid? SignatureId { get; set; } = null;
    public DateTime SignedAtUtc { get; set; }
}
