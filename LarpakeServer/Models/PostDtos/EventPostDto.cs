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
    public required DateTime StartTimeUtc { get; set; }

    public DateTime? EndTimeUtc { get; set; } = null;

    [Required]
    [MaxLength(70)]
    public string Location { get; set; } = string.Empty;

    public long LuuppiRefId { get; set; } = -1;

    [MaxLength(200)]
    public string? WebsiteUrl { get; set; } = null;

    public Image? Image { get; set; } = null;
}
