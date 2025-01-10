
namespace LarpakeServer.Models.DatabaseModels;

public class LarpakeEvent
{
    public required long Id { get; set; } 
    public required long LarpakeSectionId { get; set; } 
    public required string Title { get; set; }
    public required string Points { get; set; }
    public string? Body { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
