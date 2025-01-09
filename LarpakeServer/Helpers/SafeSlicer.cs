namespace LarpakeServer.Helpers;

public static class SafeSlicer
{
    public static string Slice(string input, int length)
    {
        return input.Length < length
            ? input : input[..length];
    }
}
