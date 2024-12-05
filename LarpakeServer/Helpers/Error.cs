namespace LarpakeServer.Helpers;

public record Error(int StatusCode, string Message, Exception? Ex = null)
{
    public void Deconstruct(out int httpStatusCode, out string message)
    {
        httpStatusCode = StatusCode;
        message = Message;
    }
}
