namespace LarpakeServer.Models.DatabaseModels;

public class AttendanceCompletion
{
    public required Guid Id { get; set; }
    public required Guid SignerUserId { get; set; }
    public required Guid EventAttendanceId { get; set; }
    public Guid? SignatureId { get; set; }
    public required DateTime ComletionTimeUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime LastModifiedUtc { get; set; }
}
