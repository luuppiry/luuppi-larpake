namespace LarpakeServer.Helpers;

public class DataError : Error
{

    public DataError(
        int StatusCode, string message, object data, 
        string dataKind, Exception? Ex = null) : base(StatusCode, message, Ex)
    {
        Data = data;
        DataKind = dataKind;
    }

    public object Data { get; }
    public string DataKind { get; }

    public static DataError AlreadyExistsNoError(object data, string dataKind, string message)
        => new(200, message, data, dataKind, null);

}
