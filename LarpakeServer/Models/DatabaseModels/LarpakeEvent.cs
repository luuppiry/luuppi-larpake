
namespace LarpakeServer.Models.DatabaseModels;

public class LarpakeEvent
{
    public required long Id { get; set; } = Constants.NullId;
    public required long LarpakeSectionId { get; set; } = Constants.NullId;
    public required string Title { get; set; }
    public required string Points { get; set; }
    public string? Body { get; set; }
    public DateTime? TimeCancelled { get; set; }
}
