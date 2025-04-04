namespace LarpakeServer.Helpers;

public enum ErrorCode
{
    Undefined = 0,

    // Id errors
    IdError = 1100,
    InvalidId = 1101,
    IdNotFound = 1102,
    KeyInvalidated = 1103,  // Key is expired and cannot be used anymore

    // Integration
    ExternalServerError = 1500,
    IntegrationDbWriteFailed = 1501,


    // Auth
    AuthenticationError = 1600,
    InvalidJWT = 1601,


    // Internal server error
    UnknownServerError = 1700,
    KeyGenFailed = 1701,



    // User action forbidden for user
    UserStatusTutor = 1801, // User is tutor (non-competing) and so cannot complete task
    UserNotAttending = 1802,    // User is not in Larpake the task is pointing to 
    CannotSignSelf = 1803, // User cannot complete task for themselves

    // Database
    DatabaseError = 1900,
    InvalidDatabaseState = 1901,    // Resource that should exists, wasn't found

}