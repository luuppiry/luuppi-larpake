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
    private readonly TokenService _tokenService;
    private readonly IUserDatabase _userDb;
    private readonly IRefreshTokenDatabase _refreshTokenDb;

    public AuthenticationController(
        TokenService generator,
        IUserDatabase userDb,
        IRefreshTokenDatabase refreshTokenDb,
        ILogger<AuthenticationController> logger) : base(logger)
    {
        _tokenService = generator;
        _userDb = userDb;
        _refreshTokenDb = refreshTokenDb;
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

        // Generate tokens
        TokenDto tokens = _tokenService.GenerateTokens(user);

        _logger.LogInformation("User {id} logged in.", user.Id);

        // Finish by saving refresh token
        return await SaveToken(user, tokens);
    }


    [HttpPost("token/refresh")]
    public async Task<IActionResult> Refresh([FromBody] TokenDto dto)
    {
        // TODO: RateLimit this, maybe limit how many tokens per user can be created

        // Validate expired access token 
        if (_tokenService.ValidateAccessToken(dto.AccessToken, out ClaimsPrincipal? claims, false) is false)
        {
            return Unauthorized();
        }

        // Get user id from token
        Guid? userId = _tokenService.GetUserId(claims);
        DateTime? expires = _tokenService.GetTokenIssuedAt(claims);
        if (userId is null || expires is null)
        {
            return Unauthorized();
        }

        // Check if possible refresh token must be expired
        if (expires.Value.Add(_tokenService.RefreshTokenLifetime) < DateTime.UtcNow)
        {
            return Unauthorized();
        }

        // Validate refresh token
        bool isValid = await _refreshTokenDb.IsValid(userId.Value, dto.RefreshToken);
        if (isValid is false)
        {
            return Unauthorized();
        }


        // Tokens are valid, generate new ones
        User? user = await _userDb.Get(userId.Value);
        if (user is null)
        {
            return IdNotFound("User does not exist.");
        }

        // Generate new tokens
        TokenDto tokens = _tokenService.GenerateTokens(user);

        _logger.LogInformation("Refreshed tokens for user {id}.", user.Id);

        // Finish by saving refresh token
        return await SaveToken(user, tokens);
    }

    private async Task<IActionResult> SaveToken(User user, TokenDto tokens)
    {
        Guard.ThrowIfNull(user);
        Guard.ThrowIfNull(tokens);
        Guard.ThrowIfNull(tokens.RefreshExpiresAt);

        // Save refresh token
        var result = await _refreshTokenDb.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = tokens.RefreshToken,
            InvalidAt = tokens.RefreshExpiresAt.Value
        });

        return result.MatchToResponse(_ => Ok(tokens), FromError);
    }
}

