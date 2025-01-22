using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class OrganizationEventPostDto
{
    [Required]
    [MinLength(5)]
    [MaxLength(80)]
    public required string Title { get; set; }

    [MaxLength(4000)]
    public string Body { get; set; } = string.Empty;

    [Required]
    public required DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; } = null;

    [Required]
    [MaxLength(100)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? ImageUrl { get; set; }

    [MaxLength(150)]
    public string? WebsiteUrl { get; set; } = null;
}
