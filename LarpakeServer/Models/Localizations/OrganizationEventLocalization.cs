using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.Localizations;

public class OrganizationEventLocalization : LocalizationBase
{
    [Required]
    [MinLength(5)]
    [MaxLength(80)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Location { get; set; }

    [MaxLength(4000)]
    public string Body { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? WebsiteUrl { get; set; } = null;

    [MaxLength(150)]
    public string? ImageUrl { get; set; } = null;
}
