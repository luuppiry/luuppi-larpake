namespace LarpakeServer.Helpers;

public class Error
{
    public Error(int statusCode, string message)
    {
        HttpStatusCode = statusCode;
        Message = message;
    }
    
    public Error(int statusCode, string message, ErrorCode errorCode) : this(statusCode, message)
    {
        ApplicationErrorCode = errorCode;
    }
    
    public Error(int statusCode, string message, Exception? ex) : this(statusCode, message)
    {
        Ex = ex;
    }

    public Error(int statusCode, string message, ErrorCode errorCode, Exception? ex) : this(statusCode, message, errorCode)
    {
        Ex = ex;
    }

    public int HttpStatusCode { get; set; }
    public string Message { get; set; }
    public Exception? Ex { get; set; }
    public ErrorCode ApplicationErrorCode { get; set; } = ErrorCode.Undefined;


    public void Deconstruct(out int httpStatusCode, out string message)
    {
        httpStatusCode = HttpStatusCode;
        message = Message;
    }
    
    public void Deconstruct(out int httpStatusCode, out string message, out Exception? ex)
    {
        httpStatusCode = HttpStatusCode;
        message = Message;
        ex = Ex;
    }

    public static Error BadRequest(string message, Exception? ex = null) => new(400, message, ex);
    public static Error BadRequest(string message, ErrorCode appError) => new(400, message, appError);
    public static Error Conflict(string message, Exception? ex = null) => new(409, message, ex);
    public static Error Conflict(string message, ErrorCode appError) => new(409, message, appError);
    public static Error NotFound(string message, Exception? ex = null) => new(404, message, ex);
    public static Error NotFound(string message, ErrorCode appError) => new(404, message, appError);
    public static Error InternalServerError(string message, Exception? ex = null) => new(500, message, ex);
    public static Error InternalServerError(string message, ErrorCode appError) => new(500, message, appError);
    public static Error Unauthorized(string? message = null, Exception? ex = null) => new(401, message ?? "Unauthorized", ex);
    public static Error Unauthorized(string? message, ErrorCode appError) => new(401, message ?? "Unauthorized", appError);
}
