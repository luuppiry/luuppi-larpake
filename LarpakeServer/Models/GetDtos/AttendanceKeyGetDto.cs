using LarpakeServer.Services;

namespace LarpakeServer.Models.GetDtos;

public class AttendanceKeyGetDto
{
    public required string Key { get; init; }
    public required string QrCodeKey { get; init; }
    public required DateTime InvalidAt { get; init; }

    public static AttendanceKeyGetDto From(AttendanceKey key, string header)
    {
        return new AttendanceKeyGetDto
        {
            Key = key.QrCodeKey,
            QrCodeKey = $"{header}{key.QrCodeKey}",
            InvalidAt = key.KeyInvalidAt
        };
    }
}
