namespace LarpakeServer.Helpers;

public enum ErrorCode
{
    Undefined = 0,

    // Id errors
    IdError = 1100,
    InvalidId = 1101,       // 
    IdNotFound = 1102,      // Id not found from database
    KeyInvalidated = 1103,  // Key is expired and cannot be used anymore
    NullId = 1104,          // Id is invalid or null (empty guid, or long -1)
    UserNotFound = 1105,    // User matching the auth token not found

    // Integration
    ExternalServerError = 1500,
    IntegrationDbWriteFailed = 1501,


    // Auth
    AuthenticationError = 1600,
    InvalidJWT = 1601,
    MalformedJWT = 1602,
    NoRefreshToken = 1603,  // Refresh token cookie not found 
    EmptyRefreshToken = 1604, // Refresh token cookie found, but it was empty

    // Internal server error
    UnknownServerError = 1700,
    KeyGenFailed = 1701,

    // User action forbidden for user
    UserStatusTutor = 1801, // User is tutor (non-competing) and so cannot complete task
    UserNotAttending = 1802,    // User is not in Larpake the task is pointing to 
    SelfActionInvalid = 1803, // User cannot do specified action to themselves (self signing or own permissions change)
    InvalidUserId = 1804,   
    RequiresHigherRole = 1805,  // Action need higher permission (for example setting other people's permissions)

    // Database
    DatabaseError = 1900,
    InvalidDatabaseState = 1901,    // Resource that should exists, wasn't found

    // Runtime action errors
    ActionNotAllowed = 2000, // Action is not allowed for some reason
    ActionNotAllowedInRuntime = 2001, // Action must be done in app configuration (e.g. setting sudo permissions)

    // SSE
    SSEError = 2100,            // Unknown error in server SSE handling
    MaxUserConnections = 2101,  // Specific user has max amount of live connections, new connections rejected
    ConnectionPoolFull = 2102,  // SSE connection pool is already full, new connections rejected

    // Data validation
    TooHighPointCount = 2201    // Point count too high, for example signature point count

}