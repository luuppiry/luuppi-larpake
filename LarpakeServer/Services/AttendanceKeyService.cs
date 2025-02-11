using LarpakeServer.Services.Options;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace LarpakeServer.Services;

public class AttendanceKeyService
{
    readonly AttendanceKeyOptions _options;

    public AttendanceKeyService(IOptions<AttendanceKeyOptions> options)
    {
        _options = options.Value;
    }

    public AttendanceKey GenerateKey()
    {
        // REMOVED 0, O and W to avoid confusion
        ReadOnlySpan<char> validCharacters = [
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 
            'I', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 
            'R', 'S', 'T', 'U', 'V', 'X', 'Y', 'Z', 
            '1', '2', '3', '4', '5', '6', '7', '8', '9'];

        Span<char> key = new char[_options.KeyLength];

        for (int i = 0; i < key.Length; i++)
        {
            int index = RandomNumberGenerator.GetInt32(0, validCharacters.Length);
            key[i] = validCharacters[index];
        }
        return new(key.ToString(), DateTime.UtcNow.AddHours(_options.KeyLifetimeHours));
    }
}
