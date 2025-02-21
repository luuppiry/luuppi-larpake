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
    IntegrationDbWriteFailed = 1501,


    // Auth
    AuthenticationError = 1600,
    InvalidJWT = 1601,


    // Internal server error
    UnknownServerError = 1700
    
}