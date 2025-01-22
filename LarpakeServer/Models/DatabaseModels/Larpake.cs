namespace LarpakeServer.Models.DatabaseModels;

public class Larpake
{
    public required long Id { get; set; }
    public required string Title { get; set; }
    public int? Year { get; set; } = null;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
