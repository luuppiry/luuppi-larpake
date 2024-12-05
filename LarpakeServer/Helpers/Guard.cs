using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LarpakeServer.Helpers;

public static class Guard
{
    [StackTraceHidden]
    public static void ThrowIfNull<T>(T? value, [CallerArgumentExpression(nameof(value))]string? name = "_value_")
    {
        if (value is null)
        {
            throw new ArgumentNullException(name);
        }
    }

}
