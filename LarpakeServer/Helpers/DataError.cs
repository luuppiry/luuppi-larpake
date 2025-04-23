namespace LarpakeServer.Helpers;

public class DataError : Error
{

    public DataError(
        int StatusCode, string message, object data, 
        string dataKind, ErrorCode code, Exception? Ex = null) : base(StatusCode, message, Ex)
    {
        Data = data;
        DataKind = dataKind;
        Code = code;
    }

    public object Data { get; }
    public string DataKind { get; }
    public ErrorCode Code { get; }

    public static DataError AlreadyExistsNoError(object data, string dataKind, string message, ErrorCode code)
        => new(200, message, data, dataKind, code, null);

}
