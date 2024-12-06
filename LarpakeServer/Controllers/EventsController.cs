using LarpakeServer.Data;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ExtendedControllerBase
{
    private readonly IEventDatabase _db;

    public EventsController(IEventDatabase db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents([FromQuery] EventQueryOptions options)
    {
        var records = await _db.Get(options);
        var result = EventsGetDto.MapFrom(records);
        
        // Calculate paging
        if (result.Events.Length == options.PageSize)
        {
            result.NextPage = options.GetNextOffset();
        }
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
    public async Task<IActionResult> CreateEvent([FromBody] EventPostDto dto)
    {
        // TODO: Add user Guid
        // TODO: validate permissions

        var record = Event.MapFrom(dto, Guid.Empty);
        Result<long> result = await _db.Insert(record);

        if (result)
        {
            return CreatedId((long)result);
        }
        return FromError(result);
    }

    [HttpPut("{eventId}")]
    public async Task<IActionResult> UpdateEvent(long eventId, [FromBody] EventPutDto dto)
    {
        // TODO: Add user Guid
        var record = Event.MapFrom(dto, Guid.Empty);
        record.Id = eventId;
        Result<int> result = await _db.Update(record);
        if (result)
        {
            return OkRowsAffected((int)result);
        }
        return FromError(result);
    }

    [HttpDelete("{eventId}")]
    public async Task<IActionResult> DeleteEvent(long eventId)
    {
        // TODO: Add user Guid
        int rowsAffected = await _db.Delete(eventId, Guid.Empty);
        return OkRowsAffected(rowsAffected);
    }

  
}
