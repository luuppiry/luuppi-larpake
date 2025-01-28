using LarpakeServer.Models.Localizations;
using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class LarpakeSectionPostDto
{
    [Required]
    [MinLength(1)]
    public required LarpakeSectionLocalization[] TextData { get; set; }

    public int OrderingWeightNumber { get; set; } = 0;
}