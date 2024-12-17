using LarpakeServer.Data;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.PostDtos;
using System.Security.Claims;

namespace LarpakeServer.Controllers;



[ApiController]
[Route("api")]
public class AuthenticationController : ExtendedControllerBase
{
    private readonly TokenService _tokenGenerator;
    private readonly IUserDatabase _userDb;

    public AuthenticationController(
        TokenService generator,
        IUserDatabase userDb,
        ILogger<AuthenticationController> logger) : base(logger)
    {
        _tokenGenerator = generator;
        _userDb = userDb;
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
        // TODO: Validate request and user

        if (dto.UserId == Guid.Empty)
        {
            return BadRequest(new
            {
                Message = "UserId must be provided."
            });
        }

        User? user = await _userDb.Get(dto.UserId);
        if (user is null)
        {
            return NotFound(new
            {
                Message = "User not found."
            });
        }

        return Ok(new
        {
            AccessToken = _tokenGenerator.GenerateToken(user)
        });
    }




    [HttpPost("token/refresh")]
    public async Task<IActionResult> Refresh([FromBody] TokenDto dto)
    {
        // Validate expired access token 
        if (_tokenGenerator.ValidateAccessToken(dto.AccessToken, out ClaimsPrincipal? claims, false) is false)
        {
            return Unauthorized();
        }

        // Get user id from token
        Guid? userId = _tokenGenerator.GetUserId(claims);
        if (userId is null)
        {
            return Unauthorized(new
            {
                Message = "Token does not contain a valid user id."
            });
        }

        // Validate refresh token
        bool isSame = await _userDb.IsSameRefreshToken(userId.Value, dto.RefreshToken);
        if (isSame is false)
        {
            return Unauthorized(new
            {
                Message = "Refresh token is invalid."
            });
        }

        // Tokens are valid, generate new ones

    }
}

