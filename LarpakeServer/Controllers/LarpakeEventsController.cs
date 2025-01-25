using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.MultipleItems;
using LarpakeServer.Models.GetDtos.SingleItem;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/larpake-events")]
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
        if (permissions.Has(Permissions.ReadAllData) is false)
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
    [RequiresPermissions(Permissions.ReadAllData)]
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
    public async Task<IActionResult> Create([FromBody] LarpakeEventPostDto record)
    {
        LarpakeEvent mapped = LarpakeEvent.From(record);
        var id = await _db.Insert(mapped);
        return id.MatchToResponse(
            ok: CreatedId, 
            error: FromError);
    }

    [HttpPut("{id}")]
    [RequiresPermissions(Permissions.CreateEvent)]
    public async Task<IActionResult> Update(long id, [FromBody] LarpakeEventPutDto record)
    {
        LarpakeEvent mapped = LarpakeEvent.From(record);
        mapped.Id = id;

        var rowsAffected = await _db.Update(mapped);
        return rowsAffected.MatchToResponse(
            ok: OkRowsAffected,
            error: FromError
            );
    }
    
    [HttpPost("{id}/cancel")]
    [RequiresPermissions(Permissions.DeleteEvent)]
    public async Task<IActionResult> Cancel(long id)
    {
        int rowsAffected = await _db.Cancel(id);
        return OkRowsAffected(rowsAffected);
    }

    [HttpPost("{id}/attendance-opportunies/{orgEventId}")]
    [RequiresPermissions(Permissions.CreateEvent)]
    public async Task<IActionResult> SyncOrganizationEvent(long id, long orgEventId)
    {
        Result result = await _db.SyncOrganizationEvent(id, orgEventId);
        return result.IsOk ? Ok() : FromError(result);
    }
    
    [HttpDelete("{id}/attendance-opportunies/{orgEventId}")]
    [RequiresPermissions(Permissions.CreateEvent)]
    public async Task<IActionResult> UnsyncOrganizationEvent(long id, long orgEventId)
    {
        int rowsAffected = await _db.UnsyncOrganizationEvent(id, orgEventId);
        return OkRowsAffected(rowsAffected);
    }

    [HttpGet("{id}/attendance-opportunies")]
    [RequiresPermissions(Permissions.CommonRead)]
    public async Task<IActionResult> GetAttendanceOpportunies(long id)
    {
        Guid[] orgEvents = await _db.GetRefOrganizationEvents(id);
        return Ok(new { OrganizationEventIds = orgEvents });
    }
    



}
