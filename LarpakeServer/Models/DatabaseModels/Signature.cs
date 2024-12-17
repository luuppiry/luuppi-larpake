using LarpakeServer.Models.PostDtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.DatabaseModels;

public class Signature
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string PathDataJson { get; set; }
    public required int Height { get; set; }
    public required int Width { get; set; }
    public int LineWidth { get; set; } = 2;
    public string? StrokeStyle { get; set; }
    public string? LineCap { get; set; }
    public DateTime CreatedAt { get; set; }

    internal static Signature From(SignaturePostDto dto)
    {
        var dataJson = JsonSerializer.Serialize(dto.Signature.Data);
        return new Signature
        {
            Id = Guid.Empty,
            UserId = dto.OwnerId,
            PathDataJson = dataJson,
            Height = dto.Signature.Height,
            Width = dto.Signature.Width,
            LineWidth = dto.Signature.LineWidth,
            StrokeStyle = dto.Signature.StrokeStyle,
            LineCap = dto.Signature.LineCap,
        };
    }
}
