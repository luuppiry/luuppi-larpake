namespace LarpakeServer.Services;

public readonly struct AttendanceKey
{
    public AttendanceKey(string key, DateTime invalidAt)
    {
        QrCodeKey = key;
        KeyInvalidAt = invalidAt;
    }

    public string QrCodeKey { get; }
    public DateTime KeyInvalidAt { get; }

    public void Deconstruct(out string key, out DateTime invalidAt)
    {
        key = QrCodeKey;
        invalidAt = KeyInvalidAt;
    }
}