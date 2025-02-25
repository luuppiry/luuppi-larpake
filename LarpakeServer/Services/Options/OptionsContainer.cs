using Microsoft.Extensions.Options;

namespace LarpakeServer.Services.Options;

public class OptionsContainer<T> : IOptions<T>
    where T : class
{
    public OptionsContainer(T value)
    {
        Value = value;
    }

    public T Value { get; }
}