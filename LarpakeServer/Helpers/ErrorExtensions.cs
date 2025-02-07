namespace LarpakeServer.Helpers;

public static class ErrorExtensions
{
    public static Error WithInner(this Error master, Error inner)
    {
        string msg = $"""
            {master.Message}
            Inner error: 
                Code: {inner.StatusCode} 
                Message: {inner.Message}
            """;
        return master with { Message = msg };
    }
}
