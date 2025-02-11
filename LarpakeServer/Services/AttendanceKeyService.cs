using LarpakeServer.Services.Implementations;
using LarpakeServer.Services.Options;
using Microsoft.Extensions.Options;

namespace LarpakeServer.Services;

public class AttendanceKeyService : RandomKeyService
{
    readonly AttendanceKeyOptions _options;

    public AttendanceKeyService(IOptions<AttendanceKeyOptions> options)
    {
        _options = options.Value;
    }

    public AttendanceKey GenerateKey()
    {
        Span<char> key = GenerateKey(stackalloc char[_options.KeyLength]);
        return new(
            key: key.ToString(), 
            invalidAt: DateTime.UtcNow.AddHours(_options.KeyLifetimeHours));
    }
}
