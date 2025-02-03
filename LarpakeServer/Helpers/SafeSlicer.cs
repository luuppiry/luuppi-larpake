namespace LarpakeServer.Helpers;

public static class SafeSlicer
{
    /// <summary>
    /// Slice span of string safely (takes max of length characters)
    /// </summary>
    /// <param name="input"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string Slice(string input, int length)
    {
        return input.Length < length
            ? input : input[..length];
    }
}
