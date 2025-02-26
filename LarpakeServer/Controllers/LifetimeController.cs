using LarpakeServer.Identity;
using System.Diagnostics;

namespace LarpakeServer.Controllers;

[ApiController]
[Route("api/lifetime")]
public class LifetimeController : ExtendedControllerBase
{
    readonly IConfiguration _configuration;

    public LifetimeController(
        IClaimsReader claimsReader,
        IConfiguration configuration,
        ILogger<ExtendedControllerBase>? logger = null) : base(claimsReader, logger)
    {
        _configuration = configuration;
    }

    record StartTimeResponse(DateTime StartTime);


    [HttpGet]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult GetLifetime()
    {
        _logger.LogInformation("User requested start time of the server.");

        string? audience = _configuration[Constants.Environment.LarpakeIdAudience];
        string? issuer = _configuration[Constants.Environment.LarpakeIdIssuer];

        _logger.LogInformation("Audience: '{audience}', Issuer: '{issuer}'.", audience, issuer);


        DateTime startTime = Process.GetCurrentProcess().StartTime;
        return Ok(new StartTimeResponse(startTime));
    }
}
