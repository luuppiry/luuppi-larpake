using System.Security.Cryptography;

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
