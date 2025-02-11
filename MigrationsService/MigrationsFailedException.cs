namespace MigrationsService;
internal class MigrationsFailedException : Exception
{
    public MigrationsFailedException() : base() { }
    public MigrationsFailedException(string message) : base(message) { }

    public MigrationsFailedException(string message, Exception innerException) 
        : base(message, innerException) { }

    public MigrationsFailedException(string filename, string? message = null, Exception? innerException = null)
        : base(message, innerException)
    {
        Filename = filename;
    }

    public string? Filename { get; init; }
}
