namespace LarpakeServer.Helpers;

/// <summary>
/// Non generic result type.
/// Used to store success or error state.
/// Error state contains information about the error.
/// </summary>
public class Result
{
    readonly Error? _error;

    /// <summary>
    /// New success result.
    /// Use <see cref="Ok"/> for success."/>
    /// </summary>
    private Result() { }

    /// <summary>
    /// New error result.
    /// Note that implicit conversion from <see cref="Error"/> exists.
    /// </summary>
    /// <param name="error"></param>
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
    ///  Get the error if the result is error, otherwise throw an exception.
    /// </summary>
    /// <param name="result"></param>
    /// <exception cref="InvalidCastException">If object is in valid state.</exception>"
    public static explicit operator Error(Result result)
    {
        if (result.IsError)
        {
            return result._error!;
        }
        throw new InvalidCastException("Valid result cannot be casted to error.");
    }

    public static implicit operator bool(Result result)
    {
        return result.IsOk;
    }

    public static readonly Result Ok = new();
    public static implicit operator Result(Error error)
    {
        return new(error);
    }
}
