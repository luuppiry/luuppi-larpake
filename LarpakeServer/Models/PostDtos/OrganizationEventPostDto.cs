using LarpakeServer.Models.Localizations;
using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class OrganizationEventPostDto
{
    [Required]
    [MinLength(1)]
    public required OrganizationEventLocalization[] TextData { get; set; } 

    [Required]
    public required DateTime StartsAt { get; set; }

    public DateTime? EndsAt { get; set; } = null;
}
