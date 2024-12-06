namespace LarpakeServer.Models.DatabaseModels;

public class FreshmanGroup
{
    public required long Id { get; set; }
    public required string Name { get; set; }
    public int? StartYear { get; set; } = null;
    public int? GroupNumber { get; set; } = null;
    public DateTime CreatedUtc { get; set; }
    public DateTime LastModifiedUtc { get; set; }
    public Guid[]? Members { get; set; }
}
