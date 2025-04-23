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
using LarpakeServer.Services.Options;
using Microsoft.Extensions.Options;
using AttendancesGetDto = LarpakeServer.Models.GetDtos.Templates.QueryDataGetDto<LarpakeServer.Models.GetDtos.AttendanceGetDto>;


namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/attendances")]
public class AttendancesController : ExtendedControllerBase
{
    readonly IAttendanceDatabase _db;
    private readonly UserService _userService;
    readonly CompletionMessageService _messageService;
    readonly ILarpakeTaskDatabase _taskDb;
    readonly AttendanceKeyOptions _keyOptions;

    public AttendancesController(
        IAttendanceDatabase db,
        UserService userService,
        CompletionMessageService messageService,
        IClaimsReader claimsReader,
        IOptions<AttendanceKeyOptions> keyOptions,
        ILarpakeTaskDatabase taskDb,
        ILogger<AttendancesController> logger) : base(claimsReader, logger)
    {
        _db = db;
        _userService = userService;
        _messageService = messageService;
        _taskDb = taskDb;
        _keyOptions = keyOptions.Value;
    }


    [HttpGet]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType<AttendancesGetDto>(200)]
    public async Task<IActionResult> Get([FromQuery] AttendanceQueryOptions options)
    {
        /* Everyone can read their own attendances,
         * all attendances can be read from tutor upwards
         */
        bool readSelfOnly = GetRequestPermissions().Has(Permissions.Tutor) is false;
        if (readSelfOnly || options.LimitToSelfOnly)
        {
            options.UserId = GetRequestUserId();
        }

        var records = await _db.Get(options);
        AttendancesGetDto result = AttendancesGetDto.MapFrom(records);
        result.SetNextPaginationPage(options);
        if (readSelfOnly)
        {
            // Add information if user can only read their own attendances
            result.Details.Add("Permissions limit to own attendances only.");
        }
        return Ok(result);
    }


    [HttpGet("{key}")]
    [RequiresPermissions(Permissions.CompleteAttendance)]
    [ProducesResponseType<FatAttendanceGetDto>(200)]
    [ProducesResponseType<ErrorMessageResponse>(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get(string key, CancellationToken token)
    {
        if (key.StartsWith(_keyOptions.Header))
        {
            key = key.Replace(_keyOptions.Header, "");
        }

        if (key.Length != _keyOptions.KeyLength)
        {
            return BadRequest($"Invalid key format, pass only {_keyOptions.KeyLength} characters.", error: ErrorCode.InvalidId);
        }

        Attendance? record = await _db.GetByKey(key);
        if (record is null)
        {
            return IdNotFound();
        }

        Result<UserGetDto> user = await _userService.GetFullUser(record.UserId, token);
        LarpakeTask? task = await _taskDb.GetTask(record.LarpakeTaskId);

        if (user.IsError)
        {
            return InternalServerError("Referenced user was not found.", ErrorCode.InvalidDatabaseState);
        }
        if (task is null)
        {
            return InternalServerError("Referenced task was not found.", ErrorCode.InvalidDatabaseState);
        }

        var attendance = FatAttendanceGetDto.From(record, (UserGetDto)user, task);
        return Ok(attendance);
    }


    [HttpPost("{eventId}")]
    [RequiresPermissions(Permissions.AttendEvent)]
    [ProducesResponseType<AttendanceKey>(200)]
    [ProducesResponseType<ErrorMessageResponse>(400)]
    [ProducesResponseType<ErrorMessageResponse>(403)]
    [ProducesResponseType<ErrorMessageResponse>(500)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> GenerateAttendanceKey(long eventId)
    {
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        var record = Attendance.From(eventId, userId);

        Result<AttendanceKey> result = await _db.GetAttendanceKey(record);
        if (result.IsError)
        {
            return FromError(result);
        }

        var key = AttendanceKeyGetDto.From((AttendanceKey)result, _keyOptions.Header);
        return Ok(key);
    }

    [HttpPost("{key}/complete")]
    [RequiresPermissions(Permissions.CompleteAttendance)]
    [ProducesResponseType<GuidIdResponse>(201)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> Complete(string key)
    {
        if (key.StartsWith(_keyOptions.Header) is false)
        {
            return BadRequest("Invalid key header.", error: ErrorCode.InvalidId);
        }
        if (key.Length != _keyOptions.ValidFullKeyLength)
        {
            return BadRequest("Invalid key length.", error: ErrorCode.InvalidId);
        }

        /* Database should here handle validation 
         * - user cannot sign their own attendance.
         */
        Guid signerId = GetRequestUserId();
        var completed = await _db.CompletedKeyed(new KeyedCompletionMetadata
        {
            // Exclude header from key
            Key = key[_keyOptions.Header.Length..],
            CompletedAt = DateTime.Now,
            SignerId = signerId
        });

        if (completed)
        {
            _messageService.SendAttendanceCompletedMessage((AttendedCreated)completed);
            return CreatedId(((AttendedCreated)completed).CompletionId);
        }
        return FromError(completed);
    }


    [HttpPost("complete")]
    [RequiresPermissions(Permissions.Admin)]
    [ProducesResponseType<GuidIdResponse>(201)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> Complete([FromBody] CompletionPutDto dto)
    {
        // User cannot sign their own attendance.
        Guid signerId = _claimsReader.ReadAuthorizedUserId(Request);
        if (dto.UserId == signerId)
        {
            return BadRequest("User cannot sign their own attendance.", error: ErrorCode.SelfActionInvalid);
        }

        var record = CompletionMetadata.From(dto, signerId);

        Result<AttendedCreated> completed = await _db.Complete(record);
        if (completed.IsError)
        {
            return FromError(completed);
        }

        _logger.LogTrace("User {user} completed event {event}.", dto.UserId, dto.EventId);

        _messageService.SendAttendanceCompletedMessage((AttendedCreated)completed);
        return CreatedId(((AttendedCreated)completed).CompletionId);
    }

    [HttpPost("uncomplete")]
    [RequiresPermissions(Permissions.DeleteAttendance)]
    [ProducesResponseType<RowsAffectedResponse>(200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> Uncomplete([FromBody] UncompletedPutDto dto)
    {
        Result<int> result = await _db.Uncomplete(dto.UserId, dto.EventId);

        _logger.IfPositive((int)result)
            .LogTrace("{admin} deleted attendance for user {user} and event {event}.",
                GetRequestUserId(), dto.UserId, dto.EventId);

        return ToResponse(result, OkRowsAffected);
    }

    [HttpPost("clean")]
    [RequiresPermissions(Permissions.Sudo)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    public async Task<IActionResult> CleanInvalidEntries()
    {
        int rowsAffected = await _db.Clean();

        _logger.LogTrace("Cleaned {count} attendances.", rowsAffected);

        return OkRowsAffected(rowsAffected);
    }
}
