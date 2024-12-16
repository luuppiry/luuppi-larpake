namespace LarpakeServer.Helpers;

public record DataError(
    int StatusCode, 
    string Message, 
    object Data, 
    string DataKind,
    Exception? Ex = null) : Error(StatusCode, Message, Ex)
{

    public static DataError AlreadyExistsNoError(object data, string dataKind, string message) 
        => new(200, message, data, dataKind, null);

}
