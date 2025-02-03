using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.Templates;
using LarpakeServer.Models.Localizations;
using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.GetDtos;

public class LarpakeGetDto : IMappable<Larpake, LarpakeGetDto>
{
    public required long Id { get; set; }
    public int? Year { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<LarpakeSection>? Sections { get; set; }
    public required LarpakeLocalization[] TextData { get; set; }

    public static LarpakeGetDto From(Larpake larpake)
    {
        return new LarpakeGetDto
        {
            Id = larpake.Id,
            TextData = larpake.TextData.ToArray(),
            Year = larpake.Year,
            CreatedAt = larpake.CreatedAt,
            UpdatedAt = larpake.UpdatedAt,
            Sections = larpake.Sections,
        };
    }
}
