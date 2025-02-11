using LarpakeServer.Models.Localizations;
using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class LarpakeEventPostDto
{
    [Required]
    public required long LarpakeSectionId { get; set; }

    [Required]
    [MinLength(1)]
    public required LarpakeEventLocalization[] TextData { get; set; }

    [Required]
    [Range(1, Constants.MaxPointsPerLarpakeEvent)]
    public required int Points { get; set; } = 1;

    public int OrderingWeightNumber { get; set; } = 0;
}
