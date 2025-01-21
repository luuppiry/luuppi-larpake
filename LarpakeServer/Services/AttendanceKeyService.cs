namespace LarpakeServer.Services;

public class AttendanceKeyService
{
    public readonly struct AttendanceKeyServiceOptions
    {
        public required string KeyLength { get; init; }
    }


    readonly AttendanceKeyServiceOptions _options;

    public AttendanceKeyService(AttendanceKeyServiceOptions options)
    {
        _options = options;
    }

    public AttendanceKey GenerateKey()
    {
        throw new NotImplementedException();
    }

}
