using LarpakeServer.Identity;

namespace LarpakeServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequiresPermissions(Permissions.CommonRead)]
public class StatisticsController : ExtendedControllerBase
{
    public StatisticsController(
        IClaimsReader claimsReader, 
        ILogger<ExtendedControllerBase>? logger = null) : base(claimsReader, logger)
    { 
    }


    [HttpGet("freshmangroup/{groupId}/total")]
    public async Task<IActionResult> GetFreshmanGroupTotal(int groupId)
    {
        var result = await _statisticsService.GetFreshmanGroupTotal(groupId);
        return Ok(result);
    }


}
