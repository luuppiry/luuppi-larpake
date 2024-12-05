namespace LarpakeServer.Helpers;

public class Error(int StatusCode, string Message)
{
    public void Deconstruct(out int httpStatusCode, out string message)
    {
        httpStatusCode = StatusCode;
        message = Message;
    }
}
