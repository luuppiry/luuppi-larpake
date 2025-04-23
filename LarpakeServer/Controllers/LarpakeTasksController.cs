using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;
using TasksGetDto = LarpakeServer.Models.GetDtos.Templates.QueryDataGetDto<LarpakeServer.Models.GetDtos.LarpakeTaskGetDto>;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/larpake-tasks")]
public class LarpakeTasksController : ExtendedControllerBase
{
    readonly record struct OrgEventIdsResponse(long[] Ids);

    readonly ILarpakeTaskDatabase _db;

    public LarpakeTasksController(
        ILarpakeTaskDatabase db, 
        IClaimsReader claimsReader, 
        ILogger<LarpakeTasksController>? logger = null) 
        : base(claimsReader, logger)
    {
        _db = db;
    }

    [HttpGet]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType(typeof(TasksGetDto), 200)]
    public async Task<IActionResult> Get([FromQuery] LarpakeTaskQueryOptions options)
    {
        Permissions permissions = GetRequestPermissions();
        bool isSelfOnly = permissions.Has(Permissions.ReadAllData) is false;
        if (isSelfOnly)
        {
            // Limit non-admins to their own events
            options.UserId = GetRequestUserId();
        }

        var records = await _db.GetTasks(options);

        // Map to result
        TasksGetDto result = TasksGetDto
            .MapFrom(records)
            .AppendPaging(options);

        if (isSelfOnly)
        {
            result.Details.Add("Read limited to attended larpakkeet only.");
        }

        return Ok(result);
    }

    [HttpGet("{taskId}")]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType(typeof(LarpakeTaskGetDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get(long taskId)
    {
        // TODO: Make sure that the user has access to this task, now I dont see it as a problem
        var record = await _db.GetTask(taskId);
        if (record is null)
        {
            return IdNotFound();
        }
        var result = LarpakeTaskGetDto.From(record);
        return Ok(result);
    }

    [HttpPost]
    [RequiresPermissions(Permissions.CreateEvent)]
    [ProducesResponseType(typeof(LongIdResponse), 201)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> Create([FromBody] LarpakeTaskPostDto record)
    {
        LarpakeTask mapped = LarpakeTask.From(record);
        var id = await _db.Insert(mapped);
        return id.ToActionResult(
            ok: CreatedId, 
            error: FromError);
    }

    [HttpPut("{taskId}")]
    [RequiresPermissions(Permissions.CreateEvent)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> Update(long taskId, [FromBody] LarpakeTaskPutDto record)
    {
        LarpakeTask mapped = LarpakeTask.From(record);
        mapped.Id = taskId;

        var rowsAffected = await _db.Update(mapped);
        return rowsAffected.ToActionResult(
            ok: OkRowsAffected,
            error: FromError);
    }
    
    [HttpPost("{taskId}/cancel")]
    [RequiresPermissions(Permissions.DeleteEvent)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    public async Task<IActionResult> Cancel(long taskId)
    {
        int rowsAffected = await _db.Cancel(taskId);
        return OkRowsAffected(rowsAffected);
    }

    [HttpPost("{taskId}/attendance-opportunities/{orgEventId}")]
    [RequiresPermissions(Permissions.CreateEvent)]
    [ProducesResponseType(200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> SyncOrganizationEvent(long taskId, long orgEventId)
    {
        Result result = await _db.SyncOrganizationEvent(taskId, orgEventId);
        return result.IsOk 
            ? Ok() : FromError(result);
    }
    
    [HttpDelete("{taskId}/attendance-opportunities/{orgEventId}")]
    [RequiresPermissions(Permissions.CreateEvent)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    public async Task<IActionResult> UnsyncOrganizationEvent(long taskId, long orgEventId)
    {
        int rowsAffected = await _db.UnsyncOrganizationEvent(taskId, orgEventId);
        return OkRowsAffected(rowsAffected);
    }

    [HttpGet("{taskId}/attendance-opportunities")]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType(typeof(OrgEventIdsResponse), 200)]
    public async Task<IActionResult> GetAttendanceOpportunies(long taskId)
    {
        long[] orgEvents = await _db.GetRefOrganizationEvents(taskId);
        return Ok(new OrgEventIdsResponse(orgEvents));
    }
    



}
