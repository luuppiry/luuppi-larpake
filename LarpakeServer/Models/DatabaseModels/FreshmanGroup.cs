namespace LarpakeServer.Models.DatabaseModels;

public class FreshmanGroup
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public int StartYear { get; set; } = -1;
    public DateTime CreatedUtc { get; set; }
    public DateTime LastModifiedUtc { get; set; }
}
