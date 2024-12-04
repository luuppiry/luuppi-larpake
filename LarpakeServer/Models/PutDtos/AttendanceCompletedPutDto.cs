namespace LarpakeServer.Models.PutDtos;

public class AttendanceCompletedPutDto
{
    public Guid? SignatureId { get; set; }
    public DateTime CompletionDateUtc { get; set; } = DateTime.UtcNow;
}
