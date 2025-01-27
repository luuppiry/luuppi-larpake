using LarpakeServer.Models.Localizations;
using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class LarpakePostDto
{
    [Required]
    [MinLength(1)]
    public required LarpakeLocalization[] TextData { get; set; }
    public int? Year { get; set; } = null;
}
