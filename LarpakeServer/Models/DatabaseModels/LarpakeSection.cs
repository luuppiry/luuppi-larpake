namespace LarpakeServer.Models.DatabaseModels;

public class LarpakeSection
{
    public required long Id { get; set; }
    public required long LarpakeId { get; set; }
    public required string Title { get; set; }
    public int SectionSequenceNumber { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
