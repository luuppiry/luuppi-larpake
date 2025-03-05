using LarpakeServer.Data;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.Templates;
using LarpakeServer.Models.QueryOptions;

namespace LarpakeServer.Controllers;

[Authorize]
[ApiController]
[Route("api/statistics")]
[RequiresPermissions(Permissions.CommonRead)]
public class StatisticsController : ExtendedControllerBase
{
    readonly IStatisticsService _statisticsService;

    public StatisticsController(
        IClaimsReader claimsReader,
        IStatisticsService statisticsService,
        ILogger<ExtendedControllerBase> logger) : base(claimsReader, logger)
    {
        _statisticsService = statisticsService;
    }

    [HttpGet("larpakkeet/own/points/average")]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType(typeof(DataResponse<LarpakeAvgPoints[]>), 200)]
    public async Task<IActionResult> GetAllAttendedLarpakeAverages()
    {
        Guid userId = GetRequestUserId();
        LarpakeAvgPoints[] records = await _statisticsService.GetAttendendLarpakeAvgPoints(userId);
        return OkData(records);
    }

    [HttpGet("larpakkeet/{larpakeId}/points/average")]
    [RequiresPermissions(Permissions.ReadStatistics)]
    [ProducesResponseType(typeof(DataResponse<LarpakeAvgPoints[]>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAllAverage(long larpakeId)
    {
        long? points = await _statisticsService.GetAveragePoints(larpakeId);
        return points is null 
            ? IdNotFound() : OkData(points);
    }

    [HttpGet("larpakkeet/own/points/total")]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType(typeof(DataResponse<LarpakeTotalPoints[]>), 200)]
    public async Task<IActionResult> GetAllAttendedLarpakeTotals()
    {
        Guid userId = GetRequestUserId();
        LarpakeTotalPoints[] records = await _statisticsService.GetAttendendLarpakeTotalPoints(userId);
        return OkData(records);
    }

    [HttpGet("larpakkeet/{larpakeId}/points/total")]
    [RequiresPermissions(Permissions.ReadStatistics)]
    [ProducesResponseType(typeof(DataResponse<long>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAllTotal(long larpakeId)
    {
        long? points = await _statisticsService.GetTotalPoints(larpakeId);
        if (points is null)
        {
            return IdNotFound();
        }
        return OkData(points);
    }

    [HttpGet("users/own/points")]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType(typeof(DataResponse<LarpakeTotalPoints[]>), 200)]
    public async Task<IActionResult> GetUserTotal()
    {
        Guid userId = GetRequestUserId();
        return await GetUserTotal(userId);
    }

    [HttpGet("users/{userId}/points")]
    [RequiresPermissions(Permissions.ReadStatistics)]
    [ProducesResponseType(typeof(DataResponse<LarpakeTotalPoints[]>), 200)]
    public async Task<IActionResult> GetUserTotal(Guid userId)
    {
        LarpakeTotalPoints[] records = await _statisticsService.GetUserPoints(userId);
        return OkData(records);
    }


    [HttpGet("users/leading")]
    [RequiresPermissions(Permissions.ReadStatistics)]
    [ProducesResponseType(typeof(QueryDataGetDto<UserPoints>), 200)]
    public async Task<IActionResult> GetLeadingUsers([FromQuery] StatisticsQueryOptions options)
    {
        UserPoints[] records = await _statisticsService.GetLeadingUsers(options);

        QueryDataGetDto<UserPoints> result = QueryDataGetDto<UserPoints>.MapFrom(records)
            .AppendPaging(options);

        return Ok(result);
    }


    [HttpGet("groups/own/points")]
    [RequiresPermissions(Permissions.CommonRead)]
    [ProducesResponseType(typeof(DataResponse<GroupTotalPoints[]>), 200)]
    public async Task<IActionResult> GetOwnGroupTotal()
    {
        Guid userId = GetRequestUserId();
        GroupTotalPoints[] records = await _statisticsService.GetGroupPoints(userId);
        return OkData(records);
    }


    [HttpGet("groups/{groupId}/points")]
    [RequiresPermissions(Permissions.ReadStatistics)]
    [ProducesResponseType(typeof(DataResponse<long>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetFreshmanGroupTotal(long groupId)
    {
        long? result = await _statisticsService.GetGroupPoints(groupId);
        return result is null
            ? IdNotFound() : OkData(result.Value);
    }

    [HttpGet("groups/leading")]
    [RequiresPermissions(Permissions.ReadStatistics)]
    [ProducesResponseType(typeof(QueryDataGetDto<GroupPoints>), 200)]
    public async Task<IActionResult> GetLeadingFreshmanGroups([FromQuery] StatisticsQueryOptions options)
    {
        GroupPoints[] records = await _statisticsService.GetLeadingGroups(options);

        var result = QueryDataGetDto<GroupPoints>.MapFrom(records)
            .AppendPaging(options);

        return Ok(result);
    }

}
