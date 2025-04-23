namespace LarpakeServer.Helpers;

internal static class ConditionedLoggerExtensions
{
    internal static ConditionedLogger<T> IfPositive<T>(this ILogger<T> logger, int value)
    {
        return value > 0 ? new(logger) : ConditionedLogger<T>._false;
    }

    internal static ConditionedLogger<T> IfNegative<T>(this ILogger<T> logger, int value)
    {
        return value < 0 ? new(logger) : ConditionedLogger<T>._false;
    }

    internal static ConditionedLogger<T> IfZero<T>(this ILogger<T> logger, int value)
    {
        return value < 0 ? new(logger) : ConditionedLogger<T>._false;
    }

    internal static ConditionedLogger<T> IfNull<T, TParam>(this ILogger<T> logger, TParam value)
    {
        return value is null ? new(logger) : ConditionedLogger<T>._false;
    }

    internal static ConditionedLogger<T> IfTrue<T>(this ILogger<T> logger, bool value)
    {
        return value is true ? new(logger) : ConditionedLogger<T>._false;
    }

    internal static ConditionedLogger<T> IfFalse<T>(this ILogger<T> logger, bool value)
    {
        return value is false ? new(logger) : ConditionedLogger<T>._false;
    }
}

/// <summary>
/// Wrapper for ILogger that only logs if a conditions are met.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class ConditionedLogger<T>
{
    private class FalseLogger<U>() : ConditionedLogger<U>;


    readonly ILogger<T>? _logger;

    internal static readonly ConditionedLogger<T> _false = new FalseLogger<T>();


    internal ConditionedLogger(ILogger<T> logger)
    {
        _logger = logger;
    }

    private ConditionedLogger() { }
    public ConditionedLogger<T> IfPositive(int value) => value > 0 ? this : _false;
    public ConditionedLogger<T> IfNegative(int value) => value < 0 ? this : _false;
    public ConditionedLogger<T> IfZero(int value) => value is 0 ? this : _false;
    public ConditionedLogger<T> IfNull<TParam>(TParam value) => value is 0 ? this : _false;
    public ConditionedLogger<T> IsTrue(bool value) => value is true ? this : _false;
    public ConditionedLogger<T> IsFalse(bool value) => value is false ? this : _false;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA2254 // Template should be a static expression

    public void LogInformation(string? template, params object?[] objects)
    {
        if (this is not FalseLogger<T>)
        {

            _logger?.LogInformation(template, objects);

        }
    }
    public void LogWarning(string? template, params object?[] objects)
    {
        if (this is not FalseLogger<T>)
        {
            _logger?.LogWarning(template, objects);
        }
    }
    public void LogError(string? template, params object?[] objects)
    {
        if (this is not FalseLogger<T>)
        {
            _logger?.LogError(template, objects);
        }
    }
    public void LogTrace(string? template, params object?[] objects)
    {
        if (this is not FalseLogger<T>)
        {
            _logger?.LogTrace(template, objects);
        }
    }

#pragma warning restore CA2254 // Template should be a static expression
#pragma warning restore IDE0079 // Remove unnecessary suppression



}
