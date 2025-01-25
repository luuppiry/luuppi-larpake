namespace LarpakeServer.Services;

public class AttendanceKey
{
    public AttendanceKey(string key, DateTime invalidAt)
    {
        QrCodeKey = key;
        KeyInvalidAt = invalidAt;
    }

    public string QrCodeKey { get; }
    public DateTime KeyInvalidAt { get; }
}