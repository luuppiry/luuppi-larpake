using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.GetDtos.Templates;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;
using OrgEventsGetDto = LarpakeServer.Models.GetDtos.Templates.QueryDataGetDto<LarpakeServer.Models.GetDtos.OrganizationEventGetDto>;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/org-events")]
public class OrganizationEventsController : ExtendedControllerBase
{
    readonly IOrganizationEventDatabase _db;

    public OrganizationEventsController(
        IOrganizationEventDatabase db,
        ILogger<OrganizationEventsController> logger,
        IClaimsReader claimsReader) : base(claimsReader, logger)
    {
        _db = db;
    }

    [HttpGet]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType(typeof(OrgEventsGetDto), 200)]
    public async Task<IActionResult> GetEvents([FromQuery] EventQueryOptions options)
    {
        if (GetRequestPermissions().Has(Permissions.Admin) is false)
        {
            options.Title = null;
        }

        var records = await _db.Get(options);

        // Map to result
        OrgEventsGetDto result = OrgEventsGetDto
            .MapFrom(records)
            .AppendPaging(options);

        return Ok(result);
    }

    [HttpGet("{eventId}")]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType(typeof(OrganizationEventGetDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEvent(long eventId)
    {
        var record = await _db.Get(eventId);
        if (record is null)
        {
            return IdNotFound();
        }
        return Ok(OrganizationEventGetDto.From(record));
    }

    [HttpPost]
    [RequiresPermissions(Permissions.CreateEvent)]
    [ProducesResponseType(typeof(LongIdResponse), 201)]
    [ProducesErrorResponseType(typeof(MessageResponse))]
    public async Task<IActionResult> CreateEvent([FromBody] OrganizationEventPostDto dto)
    {
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        var record = OrganizationEvent.MapFrom(dto, userId);
        
        Result<long> result = await _db.Insert(record);
        
        _logger.IfTrue(result)
            .LogInformation("User {userId} created event {eventId}", 
                GetRequestUserId(), (long)result);
        
        return result.MatchToResponse(
            ok: CreatedId,
            error: FromError);
    }

    [HttpPut("{eventId}")]
    [RequiresPermissions(Permissions.CreateEvent)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    [ProducesErrorResponseType(typeof(MessageResponse))]
    public async Task<IActionResult> UpdateEvent(long eventId, [FromBody] OrganizationEventPutDto dto)
    {
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        var record = OrganizationEvent.MapFrom(dto, eventId, userId);
        record.Id = eventId;
        
        Result<int> result = await _db.Update(record);
        _logger.IfTrue(result)
            .LogInformation("User {userId} updated event {eventId}", 
                GetRequestUserId(), eventId);

        return result.MatchToResponse(
            ok: OkRowsAffected, 
            error: FromError);
    }

    [HttpDelete("{eventId}")]
    [RequiresPermissions(Permissions.DeleteEvent)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    public async Task<IActionResult> DeleteEvent(long eventId)
    {
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        
        int rowsAffected = await _db.SoftDelete(eventId, userId);
        _logger.IfPositive(rowsAffected)
            .LogInformation("User {userId} deleted event {eventId}", 
                userId, eventId);

        return OkRowsAffected(rowsAffected);
    }

    [HttpDelete("{eventId}/hard")]
    [RequiresPermissions(Permissions.HardDeleteEvent)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    public async Task<IActionResult> HardDeleteEvent(long eventId)
    {
        int rowsAffected = await _db.HardDelete(eventId);
        _logger.IfPositive(rowsAffected)
            .LogInformation("User {userId} hard deleted event {eventId}", 
                GetRequestUserId(), eventId);
        
        return OkRowsAffected(rowsAffected);
    }





}
