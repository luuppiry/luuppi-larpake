namespace LarpakeServer.Services;

public record ClientPoolConfiguration(int MaxSize)
{
    public ClientPoolConfiguration() : this(1000) { }
}
