namespace LarpakeServer.Services;

public readonly struct AttendanceKey
{
    public AttendanceKey(string key, DateTime invalidAt)
    {
        Value = key;
        InvalidAt = invalidAt;
    }

    public string Value { get; }
    public DateTime InvalidAt { get; }

    public void Deconstruct(out string key, out DateTime invalidAt)
    {
        key = Value;
        invalidAt = InvalidAt;
    }
}