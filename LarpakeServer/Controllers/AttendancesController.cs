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
    readonly IAttendanceDatabase _db;
    readonly CompletionMessageService _messageService;

    public AttendancesController(
        IAttendanceDatabase db, 
        CompletionMessageService messageService,
        IClaimsReader claimsReader,
        ILogger<AttendancesController> logger) : base(claimsReader, logger)
    {
        _db = db;
        _messageService = messageService;
    }


    [HttpGet]
    [RequiresPermissions(Permissions.CommonRead)]
    public async Task<IActionResult> Get([FromQuery] AttendanceQueryOptions options)
    {
        /* Everyone can read their own attendances,
         * all attendances can be read from tutor upwards
         */
        Permissions permissions = _claimsReader.ReadAuthorizedUserPermissions(Request);
        bool readSelfOnly = permissions.Has(Permissions.Tutor) is false;
        if (readSelfOnly)
        {
            options.UserId = _claimsReader.ReadAuthorizedUserId(Request);
        }

        var records = await _db.Get(options);
        var result = AttendancesGetDto.MapFrom(records);
        result.SetNextPaginationPage(options);
        if (readSelfOnly)
        {
            // Add information if user can only read their own attendances
            result.Details.Add("Permissions limit to own attendances only.");
        }
        return Ok(result);
    }


    [HttpPost("{eventId}")]
    [RequiresPermissions(Permissions.AttendEvent)]
    public async Task<IActionResult> Post(long eventId)
    {
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        var record = Attendance.MapFrom(eventId, userId);

        Result<AttendanceKey> result = await _db.RequestAttendanceKey(record);
        return result.MatchToResponse(
            ok: x => Ok((AttendanceKey)result),
            error: FromError);
    }

    [HttpPost("complete")]
    [RequiresPermissions(Permissions.CompleteAttendance)]
    public async Task<IActionResult> Complete([FromBody] CompletionPutDto dto)
    {
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        var record = CompletionMetadata.From(dto, userId);
        
        Result<AttendedCreated> result = await _db.Complete(record);
        if (result)
        {
            _messageService.SendAttendanceCompletedMessage((AttendedCreated)result);
            return CreatedId(((AttendedCreated)result).CompletionId);
        }
        return FromError(result);
    }

    [HttpPost("uncomplete")]
    [RequiresPermissions(Permissions.DeleteAttendance)]
    public async Task<IActionResult> Uncomplete([FromBody] UncompletedPutDto dto)
    {
        Result<int> result = await _db.Uncomplete(dto.UserId, dto.EventId);
        return result.MatchToResponse(
            ok: OkRowsAffected,
            error: FromError);
    }

    [HttpPost("clean")]
    [RequiresPermissions(Permissions.Sudo)]
    public async Task<IActionResult> Clean()
    {
        int rowsAffected = await _db.Clean();
        return OkRowsAffected(rowsAffected);
    }
}
