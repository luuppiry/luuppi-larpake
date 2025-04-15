namespace LarpakeServer.Helpers;

public static class MiddlewareHelpers
{
    public static bool IsExtensionless(ReadOnlySpan<char> path)
    {
        for (int i = path.Length - 1; i >= 0; i--)
        {
            if (path[i] is '.')
            {
                return false;
            }
            if (path[i] is '/')
            {
                return true;
            }
        }
        return true;
    }
}
