using Microsoft.AspNetCore.Components.RenderTree;

namespace LarpakeServer.Helpers.Generic;

/// <summary>
/// Generic implementation of <see cref="Result"/>.
/// A type that can be either a value or an error.
/// </summary>
/// <typeparam name="T">Type of the success value.</typeparam>
public class Result<T>
{
    readonly T? _value;
    readonly Result _state;

    /// <summary>
    /// New success result with a value.
    /// </summary>
    /// <param name="value"></param>
    public Result(T value)
    {
        _value = value;
        _state = Result.Ok;
    }

    /// <summary>
    /// New error result.
    /// </summary>
    public Result(Error error)
    {
        _state = error;
    }

    /// <summary>
    /// Is the result success.
    /// </summary>
    public bool IsOk => _state.IsOk;

    /// <summary>
    /// Is the result an error.
    /// </summary>
    public bool IsError => _state.IsError;

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
        return (Error)result._state;
    }
    public static implicit operator bool(Result<T> result)
    {
        return result.IsOk;
    }

    /// <summary>
    /// Map both possible outcomes to <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="ok"></param>
    /// <param name="error"></param>
    /// <returns>Http response type <see cref="IActionResult"/>.</returns>
    public IActionResult ToActionResult(Func<T, IActionResult> ok, Func<Result<T>, IActionResult> error)
    {
        if (this)
        {
            return ok((T)this);
        }
        return error(this);
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
