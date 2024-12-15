namespace LarpakeServer.Services.Implementations;

public record ClientPoolConfiguration(int MaxSize)
{
    public ClientPoolConfiguration() : this(1000) { }
}
