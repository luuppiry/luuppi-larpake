using System.Security.Cryptography;

namespace LarpakeServer.Services.Implementations;

public abstract class RandomKeyService
{
    protected Span<char> GenerateKey(Span<char> buffer)
    {
        // REMOVED 0, O and W to avoid confusion
        ReadOnlySpan<char> validCharacters = [
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H',
            'I', 'J', 'K', 'L', 'M', 'N', 'P', 'Q',
            'R', 'S', 'T', 'U', 'V', 'X', 'Y', 'Z',
            '1', '2', '3', '4', '5', '6', '7', '8', '9'];

        return GenerateKey(buffer, validCharacters);
    }

    protected Span<char> GenerateKey(Span<char> buffer, ReadOnlySpan<char> validCharacters)
    {
       
        if (validCharacters.Length is 0)
        {
            throw new ArgumentException("validCharacters must have at least one character.");
        }

        // Generate key
        for (int i = 0; i < buffer.Length; i++)
        {
            int index = RandomNumberGenerator.GetInt32(0, validCharacters.Length);
            buffer[i] = validCharacters[index];
        }
        return buffer;
    }

}
