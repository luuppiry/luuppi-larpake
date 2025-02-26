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

    record struct StartTimeResponse(DateTime StartTime);
    record ServerInfo(string ServerVersion, string Authors, string Copyright, string Mode, string Name);


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
}
