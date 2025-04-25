using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.External;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;
using LarpakeServer.Services;
using OrgEventsGetDto = LarpakeServer.Models.GetDtos.Templates.QueryDataGetDto<LarpakeServer.Models.GetDtos.OrganizationEventGetDto>;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/org-events")]
public class OrganizationEventsController : ExtendedControllerBase
{
    readonly IOrganizationEventDatabase _db;
    readonly IExternalIntegrationService _integrationService;
    readonly IExternalDataDbService _externalDataDbService;

    public OrganizationEventsController(
        IOrganizationEventDatabase db,
        IExternalIntegrationService integrationService,
        IExternalDataDbService externalDataDbService,
        ILogger<OrganizationEventsController> logger,
        IClaimsReader claimsReader) : base(claimsReader, logger)
    {
        _db = db;
        _integrationService = integrationService;
        _externalDataDbService = externalDataDbService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OrgEventsGetDto), 200)]
    public async Task<IActionResult> GetEvents([FromQuery] EventQueryOptions options)
    {
        if (GetRequestPermissions(unauthenticatedThrows: false).Has(Permissions.Admin) is false)
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
    [AllowAnonymous]
    [ProducesResponseType(typeof(OrganizationEventGetDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEvent(long eventId)
    {
        OrganizationEvent? record = await _db.Get(eventId);
        if (record is null)
        {
            return IdNotFound();
        }
        var dto = OrganizationEventGetDto.From(record);
        return Ok(dto);
    }

    [HttpPost]
    [RequiresPermissions(Permissions.CreateEvent)]
    [ProducesResponseType(typeof(LongIdResponse), 201)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> CreateEvent([FromBody] OrganizationEventPostDto dto)
    {
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        OrganizationEvent? record = OrganizationEvent.MapFrom(dto, userId);
        
        Result<long> result = await _db.Insert(record);
        
        _logger.IfTrue(result)
            .LogTrace("User {userId} created event {eventId}", 
                GetRequestUserId(), (long)result);
        
        return result.ToActionResult(
            ok: CreatedId,
            error: FromError);
    }

    [HttpPut("{eventId}")]
    [RequiresPermissions(Permissions.CreateEvent)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> UpdateEvent(long eventId, [FromBody] OrganizationEventPutDto dto)
    {
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        var record = OrganizationEvent.MapFrom(dto, eventId, userId);
        record.Id = eventId;
        
        Result<int> result = await _db.Update(record);
        _logger.IfTrue(result)
            .LogTrace("User {userId} updated event {eventId}", 
                GetRequestUserId(), eventId);

        return result.ToActionResult(
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
            .LogTrace("User {userId} deleted event {eventId}", 
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
            .LogTrace("User {userId} hard deleted event {eventId}", 
                GetRequestUserId(), eventId);
        
        return OkRowsAffected(rowsAffected);
    }

    [HttpGet("pull-external-server-events")]
    [RequiresPermissions(Permissions.Admin)]
    public async Task<IActionResult> PullUpdateFromExternalServer(CancellationToken token)
    {

        Result<ExternalEvent[]> events = await _integrationService.PullEventsFromExternalSource(token);
        if (events.IsError)
        {
            return FromError(events);
        }

        Guid userId = GetRequestUserId();
        Result<int> result = await _externalDataDbService.SyncExternalEvents((ExternalEvent[])events, userId);
        return result.ToActionResult(
            ok: OkRowsAffected,
            error: FromError);
    }



}
