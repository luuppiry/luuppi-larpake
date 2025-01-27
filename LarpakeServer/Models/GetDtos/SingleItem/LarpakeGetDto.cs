using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos.SingleItem;

public class LarpakeGetDto
{
    public class Localized
    {
        public string Lang => "fi";
        public required string Title { get; set; }
        public string? Description { get; set; }
    }

    public required long Id { get; set; }
    public required Localized[] TextData { get; set; }
    public int? Year { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<LarpakeSection>? Sections { get; set; }

    public static LarpakeGetDto From(Larpake larpake)
    {
        return new LarpakeGetDto
        {
            Id = larpake.Id,
            TextData = [
                new Localized
            {
                Title = larpake.Title,
                Description = larpake.Description,
            }],
            Year = larpake.Year,
            CreatedAt = larpake.CreatedAt,
            UpdatedAt = larpake.UpdatedAt,
            Sections = larpake.Sections,
        };
    }
}
