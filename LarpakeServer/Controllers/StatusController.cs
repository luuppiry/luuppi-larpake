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
}
