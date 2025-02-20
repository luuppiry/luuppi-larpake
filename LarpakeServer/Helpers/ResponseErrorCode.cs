namespace LarpakeServer.Helpers;

public enum ErrorCode
{
    Undefined = 0,

    // Id errors
    IdError = 1100,
    InvalidId = 1101,
    IdNotFound = 1102,

    // Integration
    ExternalServerError = 1500,
}