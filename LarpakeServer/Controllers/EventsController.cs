using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Helpers.Generic;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EventsController : ExtendedControllerBase
{
    readonly IEventDatabase _db;
    readonly IClaimsReader _claimsReader;

    public EventsController(
        IEventDatabase db, 
        ILogger<EventsController> logger,
        IClaimsReader claimsReader) : base(logger)
    {
        _db = db;
        _claimsReader = claimsReader;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents([FromQuery] EventQueryOptions options)
    {
        var records = await _db.Get(options);
        var result = EventsGetDto.MapFrom(records);
        
        result.SetNextPaginationPage(options);
        return Ok(result);
    }

    [HttpGet("{eventId}")]
    public async Task<IActionResult> GetEvent(long eventId)
    {
        var record = await _db.Get(eventId);
        if (record is null)
        {
            return IdNotFound();
        }
        return Ok(EventGetDto.From(record));
    }

    [HttpPost]
    [RequiresPermissions(Permissions.CreateEvent)]
    public async Task<IActionResult> CreateEvent([FromBody] EventPostDto dto)
    {
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        var record = Event.MapFrom(dto, userId);
        Result<long> result = await _db.Insert(record);
        if (result)
        {
            return CreatedId((long)result);
        }
        return FromError(result);
    }

    [HttpPut("{eventId}")]
    [RequiresPermissions(Permissions.CreateEvent)]
    public async Task<IActionResult> UpdateEvent(long eventId, [FromBody] EventPutDto dto)
    {
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        var record = Event.MapFrom(dto, eventId, userId);
        record.Id = eventId;
        Result<int> result = await _db.Update(record);
        if (result)
        {
            return OkRowsAffected((int)result);
        }
        return FromError(result);
    }

    [HttpDelete("{eventId}")]
    [RequiresPermissions(Permissions.DeleteEvent)]
    public async Task<IActionResult> DeleteEvent(long eventId)
    {
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        int rowsAffected = await _db.Delete(eventId, userId);
        return OkRowsAffected(rowsAffected);
    }
}
