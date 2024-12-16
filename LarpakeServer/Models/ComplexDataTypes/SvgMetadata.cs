using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LarpakeServer.Models.ComplexDataTypes;

public class SvgMetadata
{
    [Required]
    [Range(0, 200)]
    public required int Height { get; set; }

    [Required]
    [Range(0, 400)]
    public required int Width { get; set; }

    [Required]
    public required List<List<UnsignedPoint2D>> Data { get; set; } 

    [Range(0, 10)]
    public int LineWidth { get; set; } = 2;

    [MaxLength(10)]
    public string? StrokeStyle { get; set; }

    [MaxLength(10)]
    public string? LineCap { get; set; }
}
