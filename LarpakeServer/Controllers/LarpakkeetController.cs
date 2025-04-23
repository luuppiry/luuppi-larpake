using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;
using LarpakeetGetDto = LarpakeServer.Models.GetDtos.Templates.QueryDataGetDto<LarpakeServer.Models.GetDtos.LarpakeGetDto>;
using LarpakeSectionsGetDto = LarpakeServer.Models.GetDtos.Templates.QueryDataGetDto<LarpakeServer.Models.DatabaseModels.LarpakeSection>;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/larpakkeet")]
public class LarpakkeetController : ExtendedControllerBase
{
    readonly ILarpakeDatabase _db;

    public LarpakkeetController(
        ILarpakeDatabase db,
        IClaimsReader claimsReader,
        ILogger<LarpakkeetController>? logger = null) : base(claimsReader, logger)
    {
        _db = db;
    }

    [HttpGet]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType(typeof(LarpakeetGetDto), 200)]
    public async Task<IActionResult> Get([FromQuery] LarpakeQueryOptions options)
    {
        /* Name search is only allowed for admins.
         * Actually you can add as many wildcards in the search as you want
         */

        Permissions permissions = GetRequestPermissions();

        bool isLimitedSearch = permissions.Has(Permissions.Admin) is false;
        if (isLimitedSearch)
        {
            options.Title = null;
        }

        bool isOwnSearch = permissions.Has(Permissions.ReadAllData) is false;
        if (isOwnSearch)
        {
            options.ContainsUser = GetRequestUserId();
        }



        var records = await _db.GetLarpakkeet(options);

        // Map to result
        LarpakeetGetDto result = LarpakeetGetDto
            .MapFrom(records)
            .AppendPaging(options);

        if (isLimitedSearch)
        {
            result.Details.Add("Search limited outside search by title.");
        }
        if (isOwnSearch)
        {
            result.Details.Add("Search limited to only own larpakkeet.");
        }
        return Ok(result);
    }

    [HttpGet("own")]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType(typeof(LarpakeetGetDto), 200)]
    public async Task<IActionResult> Get([FromQuery] bool? minimize)
    {
        minimize ??= false;
        var options = new LarpakeQueryOptions
        {
            DoMinimize = minimize.Value,
            ContainsUser = GetRequestUserId()
        };
        return await Get(options);
    }


    [HttpGet("{larpakeId}")]
    [RequiresPermissions(Permissions.ReadAllData)]
    [ProducesResponseType(typeof(LarpakeGetDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get(long larpakeId)
    {
        var record = await _db.GetLarpake(larpakeId);
        return record is null
            ? IdNotFound() : Ok(record);
    }

    [HttpGet("{larpakeId}/sections")]
    [RequiresPermissions(Permissions.ReadAllData)]
    [ProducesResponseType(typeof(LarpakeSectionsGetDto), 200)]
    public async Task<IActionResult> GetSections(long larpakeId, [FromQuery] QueryOptions options)
    {
        LarpakeSection[] sections = await _db.GetSections(larpakeId, options);
        LarpakeSectionsGetDto result = LarpakeSectionsGetDto.MapFrom(sections);
        result.SetNextPaginationPage(options);
        return Ok(result);
    }

    [HttpGet("section/{sectionId}")]
    [RequiresPermissions(Permissions.CommonRead)]
    public async Task<IActionResult> GetSectionById(long sectionId)
    {
        bool isSelfOnlySearch = GetRequestPermissions().Has(Permissions.ReadAllData) is false;

        LarpakeSection? section;
        if (isSelfOnlySearch)
        {
            Guid userId = GetRequestUserId();
            section = await _db.GetSectionsByIdAndUser(sectionId, userId);
            return section is null
                ? IdNotFound() : Ok(section);
        }
        else
        {
            // Admin can see all sections
            section = await _db.GetSection(sectionId);
            return section is null
                ? IdNotFound() : Ok(section);
        }
    }


    [HttpPost]
    [RequiresPermissions(Permissions.CreateLarpake)]
    [ProducesResponseType(typeof(LongIdResponse), 201)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> Create([FromBody] LarpakePostDto record)
    {
        var larpake = Larpake.From(record);
        var result = await _db.InsertLarpake(larpake);
        return result.ToActionResult(
            ok: CreatedId,
            error: FromError);
    }

    [HttpPut("{larpakeId}")]
    [RequiresPermissions(Permissions.CreateLarpake)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> Update(long larpakeId, [FromBody] LarpakePutDto record)
    {
        var larpake = Larpake.From(record);
        larpake.Id = larpakeId;
        var result = await _db.UpdateLarpake(larpake);
        return result.ToActionResult(
            ok: OkRowsAffected,
            error: FromError);
    }


    [HttpDelete("{larpakeId}")]
    [RequiresPermissions(Permissions.DeleteLarpake)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    public async Task<IActionResult> Delete(long larpakeId)
    {
        int rowsAffected = await _db.DeleteLarpake(larpakeId);
        return OkRowsAffected(rowsAffected);
    }

    [HttpPost("{larpakeId}/sections")]
    [RequiresPermissions(Permissions.CreateLarpake)]
    [ProducesResponseType(typeof(LongIdResponse), 201)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> CreateSection(long larpakeId, [FromBody] LarpakeSectionPostDto dto)
    {
        var record = LarpakeSection.From(dto, larpakeId);
        Result<long> result = await _db.InsertSection(record);
        return result.ToActionResult(
            ok: CreatedId,
            error: FromError);
    }

    [HttpPut("{larpakeId}/sections")]
    [RequiresPermissions(Permissions.CreateLarpake)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    [ProducesErrorResponseType(typeof(ErrorMessageResponse))]
    public async Task<IActionResult> UpdateSection(long larpakeId, [FromBody] LarpakeSectionPutDto dto)
    {
        var record = LarpakeSection.From(dto, larpakeId);
        Result<int> result = await _db.UpdateSection(record);
        return result.ToActionResult(
            ok: OkRowsAffected,
            error: FromError);
    }

    [HttpDelete("sections/{sectionId}")]
    [RequiresPermissions(Permissions.DeleteLarpake)]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    public async Task<IActionResult> DeleteSection(long sectionId)
    {
        int rowsAffected = await _db.DeleteSection(sectionId);
        return OkRowsAffected(rowsAffected);
    }
}
