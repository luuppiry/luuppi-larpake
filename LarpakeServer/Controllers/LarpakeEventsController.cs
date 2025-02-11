using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.GetDtos.Templates;
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
        bool isSelfOnly = permissions.Has(Permissions.ReadAllData) is false;
        if (isSelfOnly)
        {
            // Limit non-admins to their own events
            options.UserId = GetRequestUserId();
        }

        var records = await _db.GetEvents(options);

        // Map to result
        var result = QueryDataGetDto<LarpakeEventGetDto>
            .MapFrom(records)
            .AppendPaging(options);

        if (isSelfOnly)
        {
            result.Details.Add("Read limited to attended larpakkeet only.");
        }

        return Ok(result);
    }

    [HttpGet("{eventId}")]
    [RequiresPermissions(Permissions.ReadAllData)]
    public async Task<IActionResult> Get(long eventId)
    {
        var record = await _db.GetEvent(eventId);
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

    [HttpPut("{eventId}")]
    [RequiresPermissions(Permissions.CreateEvent)]
    public async Task<IActionResult> Update(long eventId, [FromBody] LarpakeEventPutDto record)
    {
        LarpakeEvent mapped = LarpakeEvent.From(record);
        mapped.Id = eventId;

        var rowsAffected = await _db.Update(mapped);
        return rowsAffected.MatchToResponse(
            ok: OkRowsAffected,
            error: FromError);
    }
    
    [HttpPost("{eventId}/cancel")]
    [RequiresPermissions(Permissions.DeleteEvent)]
    public async Task<IActionResult> Cancel(long eventId)
    {
        int rowsAffected = await _db.Cancel(eventId);
        return OkRowsAffected(rowsAffected);
    }

    [HttpPost("{eventId}/attendance-opportunities/{orgEventId}")]
    [RequiresPermissions(Permissions.CreateEvent)]
    public async Task<IActionResult> SyncOrganizationEvent(long eventId, long orgEventId)
    {
        Result result = await _db.SyncOrganizationEvent(eventId, orgEventId);
        return result.IsOk 
            ? Ok() : FromError(result);
    }
    
    [HttpDelete("{eventId}/attendance-opportunities/{orgEventId}")]
    [RequiresPermissions(Permissions.CreateEvent)]
    public async Task<IActionResult> UnsyncOrganizationEvent(long eventId, long orgEventId)
    {
        int rowsAffected = await _db.UnsyncOrganizationEvent(eventId, orgEventId);
        return OkRowsAffected(rowsAffected);
    }

    [HttpGet("{eventId}/attendance-opportunities")]
    [RequiresPermissions(Permissions.CommonRead)]
    public async Task<IActionResult> GetAttendanceOpportunies(long eventId)
    {
        Guid[] orgEvents = await _db.GetRefOrganizationEvents(eventId);
        return Ok(new { OrganizationEventIds = orgEvents });
    }
    



}
