using LarpakeServer.Models.PostDtos;

namespace LarpakeServer.Models.DatabaseModels;

public class Larpake
{
    public required long Id { get; set; }
    public required string Title { get; set; }
    public int? Year { get; set; } = null;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<LarpakeSection>? Sections { get; set; }

    internal static Larpake From(LarpakePostDto record)
    {
        return new Larpake
        {
            Id = Constants.NullId,
            Title = record.Title,
            Year = record.Year,
            Description = record.Description,
        };
    }
    
  
}
