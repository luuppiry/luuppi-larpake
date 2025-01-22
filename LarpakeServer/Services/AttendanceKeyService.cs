namespace LarpakeServer.Services;

public class AttendanceKeyService
{
    public class AttendanceKeyServiceOptions
    {
        public required int KeyLength { get; init; }
        public required int KeyLifetimeHours { get; init; }
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
