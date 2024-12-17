using LarpakeServer.Models.ComplexDataTypes;
using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class EventPostDto
{
    [Required]
    [MinLength(5)]
    [MaxLength(50)]
    public required string Title { get; set; }

    public string Body { get; set; } = string.Empty;

    [Required]
    public required DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; } = null;

    [Required]
    [MaxLength(70)]
    public string Location { get; set; } = string.Empty;

    public long LuuppiRefId { get; set; } = Constants.NullId;

    [MaxLength(200)]
    public string? WebsiteUrl { get; set; } = null;

    public Image? Image { get; set; } = null;
}
