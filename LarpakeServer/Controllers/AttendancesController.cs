using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.DatabaseModels.Metadata;
using LarpakeServer.Models.EventModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;
using LarpakeServer.Services;

namespace LarpakeServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendancesController : ExtendedControllerBase
{
    private readonly IAttendanceDatabase _db;
    private readonly CompletionMessageService _messageService;

    public AttendancesController(IAttendanceDatabase db, CompletionMessageService messageService)
    {
        _db = db;
        _messageService = messageService;
    }


    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] AttendanceQueryOptions options)
    {
        var records = await _db.Get(options);
        var result  = AttendancesGetDto.MapFrom(records);
        result.CalculateNextPageFrom(options);
        return Ok(result);
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AttendancePostDto dto)
    {
        // TODO: Add client Guid
        var record = Attendance.MapFrom(dto);
        Result<int> result = await _db.InsertUncompleted(record);
        if (result)
        {
            return OkRowsAffected((int)result);
        }
        return FromError(result);
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] CompletedPutDto dto)
    {
        // TODO: Add user Guid
        var record = AttendanceCompletionMetadata.From(dto, Guid.Empty);
        Result<AttendedCreated> result = await _db.Complete(record);
        if (result)
        {
            _messageService.SendAttendanceCompletedMessage((AttendedCreated)result);
            return CreatedId(((AttendedCreated)result).CompletionId);
        }
        return FromError(result);
    }

    [HttpPost("uncomplete")]
    public async Task<IActionResult> Uncomplete([FromBody] UncompletedPutDto dto)
    {
        Result<int> result = await _db.Uncomplete(dto.UserId, dto.EventId);
        if (result)
        {
            return OkRowsAffected((int)result);
        }
        return FromError(result);
    }

}
