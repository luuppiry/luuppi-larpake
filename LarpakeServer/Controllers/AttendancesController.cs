using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.DatabaseModels.Metadata;
using LarpakeServer.Models.EventModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;
using LarpakeServer.Services;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AttendancesController : ExtendedControllerBase
{
    private readonly IAttendanceDatabase _db;
    private readonly CompletionMessageService _messageService;
    private readonly IClaimsReader _claimsReader;

    public AttendancesController(
        IAttendanceDatabase db, 
        CompletionMessageService messageService,
        IClaimsReader claimsReader,
        ILogger<AttendancesController> logger) : base(logger)
    {
        _db = db;
        _messageService = messageService;
        _claimsReader = claimsReader;
    }


    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] AttendanceQueryOptions options)
    {
        var records = await _db.Get(options);
        var result  = AttendancesGetDto.MapFrom(records);
        result.CalculateNextPageFrom(options);
        return Ok(result);
    }


    [HttpPost("{eventId}")]
    [RequiresPermissions(Permissions.AttendEvent)]
    public async Task<IActionResult> Post(long eventId)
    {
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        var record = Attendance.MapFrom(eventId, userId);

        Result<int> result = await _db.InsertUncompleted(record);
        if (result)
        {
            return OkRowsAffected((int)result);
        }
        return FromError(result);
    }

    [HttpPost("complete")]
    [RequiresPermissions(Permissions.CompleteAttendance)]
    public async Task<IActionResult> Complete([FromBody] CompletedPutDto dto)
    {
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        var record = AttendanceCompletionMetadata.From(dto, userId);
        
        Result<AttendedCreated> result = await _db.Complete(record);
        if (result)
        {
            _messageService.SendAttendanceCompletedMessage((AttendedCreated)result);
            return CreatedId(((AttendedCreated)result).CompletionId);
        }
        return FromError(result);
    }

    [HttpPost("uncomplete")]
    [RequiresPermissions(Permissions.Admin)]
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
