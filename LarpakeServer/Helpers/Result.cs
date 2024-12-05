namespace LarpakeServer.Helpers;

/// <summary>
/// Result type that can be either a value or an error.
/// </summary>
/// <typeparam name="T">Type of the success value.</typeparam>
public class Result<T>
{
    readonly T? _value;
    readonly Error? _error;

    /// <summary>
    /// New success result with a value.
    /// </summary>
    /// <param name="value"></param>
    public Result(T value)
    {
        _value = value;
    }

    /// <summary>
    /// New error result.
    /// </summary>
    public Result(Error error)
    {
        _error = error;
    }

    /// <summary>
    /// Is the result success.
    /// </summary>
    public bool IsOk => _error is null;

    /// <summary>
    /// Is the result an error.
    /// </summary>
    public bool IsError => !IsOk;

    /// <summary>
    /// Get the value if the result is ok, otherwise throw an exception
    /// </summary>
    /// <param name="result"></param>
    /// <exception cref="InvalidCastException">If object is in error state.</exception>
    public static explicit operator T(Result<T> result)
    {
        if (result.IsOk)
        {
            return result._value!;
        }
        throw new InvalidCastException("Error result cannot be casted to valid.");
    }
    /// <summary>
    ///  Get the error if the result is error, otherwise throw an exception
    /// </summary>
    /// <param name="result"></param>
    /// <exception cref="InvalidCastException">If object is in valid state.</exception>"
    public static explicit operator Error(Result<T> result)
    {
        if (result.IsError)
        {
            return result._error!;
        }
        throw new InvalidCastException("Valid result cannot be casted to error.");
    }

    public static implicit operator bool(Result<T> result)
    {
        return result.IsOk;
    }



    public static implicit operator Result<T>(T value)
    {
        return new(value);
    }
    public static implicit operator Result<T>(Error error)
    {
        return new(error);
    }
}
