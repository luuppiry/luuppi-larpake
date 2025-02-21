namespace LarpakeServer.Helpers;

public static class ErrorExtensions
{
    public static Error WithInner(this Error master, Error inner)
    {
        master.Message = $"""
            {master.Message}
            Inner error: 
                Code: {inner.HttpStatusCode} 
                Message: {inner.Message}
            """;
        return master;
    }
}
