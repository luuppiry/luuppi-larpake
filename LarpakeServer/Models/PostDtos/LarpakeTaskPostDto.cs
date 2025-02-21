using LarpakeServer.Models.Localizations;
using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class LarpakeTaskPostDto
{
    [Required]
    public required long LarpakeSectionId { get; set; }

    [Required]
    [MinLength(1)]
    public required LarpakeTaskLocalization[] TextData { get; set; }

    [Required]
    [Range(1, Constants.MaxPointsPerLarpakeTask)]
    public required int Points { get; set; } = 1;

    public int OrderingWeightNumber { get; set; } = 0;
}
