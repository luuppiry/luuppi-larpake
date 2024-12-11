namespace LarpakeServer.Helpers;

public record Error(int StatusCode, string Message, Exception? Ex = null)
{
    public void Deconstruct(out int httpStatusCode, out string message)
    {
        httpStatusCode = StatusCode;
        message = Message;
    }
    
    public void Deconstruct(out int httpStatusCode, out string message, out Exception? ex)
    {
        httpStatusCode = StatusCode;
        message = Message;
        ex = Ex;
    }

    public static Error BadRequest(string message, Exception? ex = null) => new(400, message, ex);
    public static Error Conflict(string message, Exception? ex = null) => new(409, message, ex);
    public static Error NotFound(string message, Exception? ex = null) => new(404, message, ex);
    public static Error InternalServerError(string message, Exception? ex = null) => new(500, message, ex);



        
        
}
