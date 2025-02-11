using System.Diagnostics.CodeAnalysis;

namespace LarpakeServer.Services;

public class AttendanceKey
{
    public AttendanceKey() { }

    [SetsRequiredMembers]
    public AttendanceKey(string key, DateTime invalidAt)
    {
        QrCodeKey = key;
        KeyInvalidAt = invalidAt;
    }

    public required string QrCodeKey { get; set; }
    public required DateTime KeyInvalidAt { get; set; }
}