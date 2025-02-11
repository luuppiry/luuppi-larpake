namespace LarpakeServer.Models.DatabaseModels;

public class Completion
{
    public required Guid Id { get; set; }
    public required Guid SignerId { get; set; }
    public Guid? SignatureId { get; set; }
    public required DateTime CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
