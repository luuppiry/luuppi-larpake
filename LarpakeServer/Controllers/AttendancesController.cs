﻿using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.DatabaseModels.Metadata;
using LarpakeServer.Models.EventModels;
using LarpakeServer.Models.GetDtos.MultipleItems;
using LarpakeServer.Models.GetDtos.SingleItem;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;
using LarpakeServer.Services;
using LarpakeServer.Services.Options;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AttendancesController : ExtendedControllerBase
{
    readonly IAttendanceDatabase _db;
    readonly CompletionMessageService _messageService;
    readonly AttendanceKeyOptions _keyOptions;

    public AttendancesController(
        IAttendanceDatabase db,
        CompletionMessageService messageService,
        IClaimsReader claimsReader,
        AttendanceKeyOptions keyOptions,
        ILogger<AttendancesController> logger) : base(claimsReader, logger)
    {
        _db = db;
        _messageService = messageService;
        _keyOptions = keyOptions;
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
    public async Task<IActionResult> Complete(string key)
    {
        if (key.StartsWith(_keyOptions.Header) is false)
        {
            return BadRequest("Invalid key header.");
        }
        if (key.Length != _keyOptions.ValidFullKeyLength)
        {
            return BadRequest("Invalid key length.");
        }

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
    [RequiresPermissions(Permissions.CompleteAttendance)]
    public async Task<IActionResult> Complete([FromBody] CompletionPutDto dto)
    {
        Guid signerId = _claimsReader.ReadAuthorizedUserId(Request);
        var record = CompletionMetadata.From(dto, signerId);
        
        Result<AttendedCreated> completed = await _db.Complete(record);
        if (completed)
        {
            _messageService.SendAttendanceCompletedMessage((AttendedCreated)completed);
            return CreatedId(((AttendedCreated)completed).CompletionId);
        }
        return FromError(completed);
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
