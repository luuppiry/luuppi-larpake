using LarpakeServer.Data;
using LarpakeServer.Helpers;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.QueryOptions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;

namespace LarpakeServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
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
        return Ok(result);
    }

    [HttpGet("{eventId}")]
    public async Task<IActionResult> GetEvent(long eventId)
    {
        var record = await _db.Get(eventId);
        if (record is null)
        {
            return NotFound(new { Message = "Id not found." });
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
            long id = (long)result;
            string? resourceUrl = Request.Path.Value;
            return Created(resourceUrl, new { Id = id });
        }

        var (statusCode, message) = (Error)result;
        return StatusCode(statusCode, message);
    }




}
