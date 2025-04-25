using LarpakeServer.Identity;
using LarpakeServer.Services.Options;
using Microsoft.AspNetCore.RateLimiting;
using System.Diagnostics;

namespace LarpakeServer.Controllers;

[ApiController]
[EnableRateLimiting(RateLimitingOptions.GeneralPolicyName)]
[Route("api/status")]
public class StatusController : ExtendedControllerBase
{

    public StatusController(
        IClaimsReader claimsReader,
        ILogger<ExtendedControllerBase>? logger = null) : base(claimsReader, logger)
    {
    }

    readonly record struct StartTimeResponse(DateTime StartTime);
    record ServerInfo(string ServerVersion, string Authors, string Copyright, string Mode, string Name);
    readonly record struct PermissionCollection(Permissions Freshman, Permissions Tutor, Permissions Admin, Permissions Sudo);
    readonly record struct RolesResponse(PermissionCollection Roles);



    [HttpGet("uptime")]
    [ProducesResponseType(typeof(StartTimeResponse), 200)]
    public IActionResult GetLifetime()
    {
        DateTime startTime = Process.GetCurrentProcess().StartTime;
        return Ok(new StartTimeResponse(startTime));
    }



    [HttpGet]
    public IActionResult GetServerInfo()
    {
        string version = Constants.Api.Version;
        string authors = Constants.Api.Authors;
        string copyright = Constants.Api.Copyright;
        string mode = Constants.Api.Mode;
        string name = Constants.Api.AppName;

        return Ok(new ServerInfo(version, authors, copyright, mode, name));
    }


    [HttpGet("permissions")]
    [ProducesResponseType<RolesResponse>(200)]
    public IActionResult GetPermissionsValues()
    {
        return Ok(new RolesResponse
        {
            Roles = new PermissionCollection
            {
                Freshman = Permissions.Freshman,
                Tutor = Permissions.Tutor,
                Admin = Permissions.Admin,
                Sudo = Permissions.Sudo,
            }
        });
    }


    record ErrorHelp(ErrorCode ErrorCode, string Description);
    record ErrorSection(string SectionName, ErrorHelp[] Errors);


    [HttpGet("error-help")]
    public IActionResult GetErrorHelp()
    {
        ErrorSection[] help = [
            new ErrorSection("general", [
                new ErrorHelp(ErrorCode.Undefined, "Undefined error, probably unknown error")
                ]),

            new ErrorSection("id errors", [
                new ErrorHelp(ErrorCode.IdError, "General id related issue"),
                new ErrorHelp(ErrorCode.InvalidId, "Id or key in invalid format or otherwise invalid"),
                new ErrorHelp(ErrorCode.IdNotFound, "Id not found from database"),
                new ErrorHelp(ErrorCode.KeyInvalidated, "Key is expired and cannot be used anymore"),
                new ErrorHelp(ErrorCode.NullId, "Id is invalid or null (empty guid, or long -1)"),
                new ErrorHelp(ErrorCode.UserNotFound, "User matching the auth token not found"),
                new ErrorHelp(ErrorCode.IdConflict, "Id already exists in database and cannot be reused"),
                ]),

            new ErrorSection("integration errors", [
                new ErrorHelp(ErrorCode.ExternalServerError, "External service provider failed to respond correctly"),
                new ErrorHelp(ErrorCode.IntegrationDbWriteFailed, "Writing values from external service to application datbase failed"),
                ]),
            
            new ErrorSection("authentication errors", [
                new ErrorHelp(ErrorCode.AuthenticationError, "General authentication related error"),
                new ErrorHelp(ErrorCode.InvalidJWT, "Json web token data is invalid"),
                new ErrorHelp(ErrorCode.MalformedJWT, "Json web token malformed"),
                new ErrorHelp(ErrorCode.NoRefreshToken, "Refresh token cookie not found"),
                new ErrorHelp(ErrorCode.EmptyRefreshToken, "Refresh token cookie found, but it was empty"),
                ]),

            new ErrorSection("internal server errors", [
                new ErrorHelp(ErrorCode.UnknownServerError, "error caused by exception that is probably unknown to the developer"),
                new ErrorHelp(ErrorCode.IntegrationDbWriteFailed, "Generation of a unique key failed which caused database key conflict"),
                ]),
            
            new ErrorSection("user action forbidden", [
                new ErrorHelp(ErrorCode.UserStatusTutor, "User is tutor (non-competing) and so cannot complete task"),
                new ErrorHelp(ErrorCode.UserNotAttending, "User is not in Larpake the task is pointing to"),
                new ErrorHelp(ErrorCode.SelfActionInvalid, "User cannot do specified action to themselves (self signing or own permissions change)"),
                new ErrorHelp(ErrorCode.RequiresHigherRole, "Action need higher permission (for example setting other people's permissions)"),
                new ErrorHelp(ErrorCode.InvalidOrganization, "User is not in correct user grop, e.g. in same larpake as fuksi to sign attendance"),
                ]),
            
            new ErrorSection("database", [
                new ErrorHelp(ErrorCode.DatabaseError, "General database error"),
                new ErrorHelp(ErrorCode.InvalidDatabaseState, "Resource that previousle existed, wasn't found"),
                ]),
            
            new ErrorSection("runtime action errors", [
                new ErrorHelp(ErrorCode.ActionNotAllowed, "Action is not allowed for some reason"),
                new ErrorHelp(ErrorCode.ActionNotAllowedInRuntime, "Action must be done in app configuration (e.g. setting sudo permissions)"),
                ]),
            
            new ErrorSection("server sent events", [
                new ErrorHelp(ErrorCode.SSEError, "General SSE error"),
                new ErrorHelp(ErrorCode.MaxUserConnections, "Specific user has too many live connections, new SSE connections are refused"),
                new ErrorHelp(ErrorCode.ConnectionPoolFull, "SSE connection pool is already full, new connections rejected"),
                ]),


            new ErrorSection("data validation", [
                new ErrorHelp(ErrorCode.TooHighPointCount, "Point count too high, for example signature point count"),
                new ErrorHelp(ErrorCode.DataFieldNull, "Data field is null, but it should not be"),
                ]),
            ];

        return Ok(help);
    }

}
