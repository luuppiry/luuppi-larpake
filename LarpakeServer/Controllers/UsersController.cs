using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;
using LarpakeServer.Models.QueryOptions;

using User_ = LarpakeServer.Models.DatabaseModels.User;

namespace LarpakeServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ExtendedControllerBase
{
    private readonly IUserDatabase _db;

    public UsersController(IUserDatabase db)
    {
        _db = db;
    }


    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryOptions options)
    {
        var records = await _db.Get(options);
        var result = UsersGetDto.MapFrom(records);
        result.CalculateNextPageFrom(options);
        return Ok(result);
    }


    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUser(Guid userId)
    {
        var record = await _db.Get(userId);
        if (record is null)
        {
            return NotFound();
        }
        return Ok(record);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserPostDto dto)
    {
        var record = User_.MapFrom(dto);
        
        Result<Guid> result = await _db.Insert(record);
        if (result)
        {
            return CreatedId((Guid)result);
        }
        return FromError(result);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UserPutDto dto)
    {
        var record = User_.MapFrom(dto);
        record.Id = userId;

        Result<int> result = await _db.Update(record);
        if (result)
        {
            return OkRowsAffected((int)result);
        }
        return FromError(result);
    }

    [HttpPut("{userId}/permissions")]
    public async Task<IActionResult> UpdateUserPermissions(Guid userId, [FromBody] UserPermissionsPutDto dto)
    {
        var record = User_.MapFrom(dto);
        record.Id = userId;

        Result<int> result = await _db.UpdatePermissions(userId, dto.Permissions);
        if (result)
        {
            return OkRowsAffected((int)result);
        }
        return FromError(result);
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        int rowsAffected = await _db.Delete(userId);
        return OkRowsAffected(rowsAffected);
    }

}
