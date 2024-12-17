using LarpakeServer.Models.ComplexDataTypes;
using LarpakeServer.Models.DatabaseModels;
using System.Text.Json;

namespace LarpakeServer.Models.GetDtos;

public class SignatureGetDto
{
    public required Guid Id { get; set; }
    public required Guid OwnerId { get; set; }
    public required SvgMetadata Signature { get; set; }
    public DateTime CreatedAt { get; set; }

    public static SignatureGetDto From(Signature record)
    {
        var data = JsonSerializer.Deserialize<List<List<UnsignedPoint2D>>>(record.PathDataJson)
            ?? throw new InvalidOperationException("Failed to deserialize svg path data.");

        return new SignatureGetDto
        {
            Id = record.Id,
            OwnerId = record.UserId,
            Signature = new SvgMetadata
            {
                Height = record.Height,
                Width = record.Width,
                Data = data,
                LineWidth = record.LineWidth,
                StrokeStyle = record.StrokeStyle,
                LineCap = record.LineCap,
            }
        };
    }
}
