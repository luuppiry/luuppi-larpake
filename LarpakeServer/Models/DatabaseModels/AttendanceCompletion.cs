namespace LarpakeServer.Models.DatabaseModels;

public class AttendanceCompletion
{
    public required Guid Id { get; set; }
    public required Guid SignerId { get; set; }
    public Guid? SignatureId { get; set; }
    public required DateTime CompletionTimeUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
