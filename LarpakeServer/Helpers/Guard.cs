using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LarpakeServer.Helpers;

public static class Guard
{

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the <paramref name="value"/> is <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">Value to be compared to null.</param>
    /// <param name="paramName">Automatically inserted <paramref name="value"/> name from caller argument expression.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
    [StackTraceHidden]
    public static void ThrowIfNull<T>([NotNull]T? value, [CallerArgumentExpression(nameof(value))]string? paramName = "_paramName_")
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }


    [StackTraceHidden]
    public static void ThrowIfNegative(int value, [CallerArgumentExpression(nameof(value))] string? paramName = "_paramName_")
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Value must be non-negative.");
        }
    }

}
