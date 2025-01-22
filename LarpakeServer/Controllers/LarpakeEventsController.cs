using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LarpakeEventsController : ExtendedControllerBase
{
    readonly ILarpakeEventDatabase _db;

    public LarpakeEventsController(
        ILarpakeEventDatabase db, 
        IClaimsReader claimsReader, 
        ILogger<LarpakeEventsController>? logger = null) 
        : base(claimsReader, logger)
    {
        _db = db;
    }

    [HttpGet]
    [RequiresPermissions(Permissions.CommonRead)]
    public async Task<IActionResult> Get([FromQuery] LarpakeEventQueryOptions options)
    {
        Permissions permissions = GetRequestPermissions();
        if (permissions.Has(Permissions.ReadAnyYearData) is false)
        {
            // Limit non-admins to their own events
            options.UserId = GetRequestUserId();
        }

        var records = await _db.GetEvents(options);
        var result = LarpakeEventsGetDto.MapFrom(records);
        result.SetNextPaginationPage(options);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [RequiresPermissions(Permissions.ReadAnyYearData)]
    public async Task<IActionResult> Get(long id)
    {
        var record = await _db.GetEvent(id);
        if (record is null)
        {
            return IdNotFound();
        }
        var result = LarpakeEventGetDto.From(record);
        return Ok(result);
    }

    [HttpPost]
    [RequiresPermissions(Permissions.CreateEvent)]
}
