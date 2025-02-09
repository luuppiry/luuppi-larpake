using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Services.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using DbUser = LarpakeServer.Models.DatabaseModels.User;

namespace LarpakeServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(RateLimitingOptions.AuthPolicyName)]
public class AuthenticationController : ExtendedControllerBase
{
    readonly TokenService _tokenService;
    readonly IUserDatabase _userDb;
    readonly IRefreshTokenDatabase _refreshTokenDb;
    const string RefreshTokenCookieName = "__Secure-refreshToken";

    record TokenInfo(string Message, string AccessToken, DateTime AccessTokenExpiresAt, DateTime RefreshTokenExpiresAt);


    public AuthenticationController(
        TokenService generator,
        IUserDatabase userDb,
        IRefreshTokenDatabase refreshTokenDb,
        IClaimsReader claimsReader,
        ILogger<AuthenticationController> logger) : base(claimsReader, logger)
    {
        _tokenService = generator;
        _userDb = userDb;
        _refreshTokenDb = refreshTokenDb;
    }

    [Authorize(AuthenticationSchemes = Constants.Auth.EntraIdScheme)]
    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        Guid entraId = ReadEntraId();

        // Check if user exists in database
        DbUser? user = await _userDb.GetByEntraId(entraId);
        if (user is null)
        {
            // User is new and must be created first
            Result<DbUser> createdUser = await SetupNewUser(entraId);
            if (createdUser.IsError)
            {
                return FromError(createdUser);
            }
            
            _logger.LogInformation("Created user {userId}.", ((DbUser)createdUser).Id);

            user = (DbUser)createdUser;
        }

        // Generate tokens as normal
        TokenGetDto tokens = _tokenService.GenerateTokens(user);

        _logger.LogInformation("User {id} logged in.", user.Id);

        return await SaveToken(user, tokens, Guid.Empty);
    }




#if DEBUG
    [AllowAnonymous]    
    [HttpPost("login/dummy")]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
        // TODO: Remove this method in production

        if (dto.UserId == Guid.Empty)
        {
            return BadRequest(new
            {
                Message = "UserId must be provided."
            });
        }

        DbUser? user = await _userDb.GetByUserId(dto.UserId);
        if (user is null)
        {
            return NotFound(new
            {
                Message = "User not found."
            });
        }

        // Generate tokens
        TokenGetDto tokens = _tokenService.GenerateTokens(user);

        _logger.LogInformation("User {id} logged in.", user.Id);

        // Finish by saving refresh token
        return await SaveToken(user, tokens, Guid.Empty);
    }
#endif

    [AllowAnonymous]    // Authentication is handled inside the method, Anonymous is ok here.
    [HttpGet("token/refresh")]
    public async Task<IActionResult> Refresh()
    {
        // Read headers and cookies to get tokens
        if (Request.Headers.TryGetValue("Authorization", out StringValues accessTokenValue) is false)
        {
            return Unauthorized();
        }
        if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out string? refreshToken) is false)
        {
            return Unauthorized();
        }

        // Get tokens and validate not empty
        string? accessToken = accessTokenValue;
        if (string.IsNullOrEmpty(accessToken))
        {
            return Unauthorized();
        }
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized();
        }

        // Validate expired access token 
        if (_tokenService.ValidateAccessToken(accessToken, out ClaimsPrincipal? claims, false) is false)
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
        var validation = await _refreshTokenDb.IsValid(userId.Value, refreshToken);
        if (validation.IsValid is false)
        {
            return Unauthorized();
        }


        // Tokens are valid, generate new ones
        DbUser? user = await _userDb.GetByUserId(userId.Value);
        if (user is null)
        {
            return IdNotFound("User does not exist.");
        }

        // Generate new tokens
        TokenGetDto newTokens = _tokenService.GenerateTokens(user);

        _logger.LogInformation("Refreshed tokens for user {id}.", user.Id);

        // Finish by saving refresh token
        return await SaveToken(user, newTokens, validation.TokenFamily.Value);
    }


    [Authorize]
    [DisableRateLimiting]
    [HttpPost("token/invalidate")]
    public async Task<IActionResult> InvalidateTokens()
    {
        Guid userId = _claimsReader.ReadAuthorizedUserId(Request);
        int rowsAffected = await _refreshTokenDb.RevokeUserTokens(userId);
        return OkRowsAffected(rowsAffected);
    }

    [Authorize]
    [DisableRateLimiting]
    [HttpPost("token/invalidate/{tokenFamily}")]
    [RequiresPermissions(Permissions.Admin)]
    public async Task<IActionResult> InvalidateTokenFamily(Guid tokenFamily)
    {
        int rowsAffected = await _refreshTokenDb.RevokeFamily(tokenFamily);
        return OkRowsAffected(rowsAffected);
    }


    private async Task<IActionResult> SaveToken(DbUser user, TokenGetDto tokens, Guid tokenFamily)
    {
        Guard.ThrowIfNull(user);
        Guard.ThrowIfNull(tokens);
        Guard.ThrowIfNull(tokens.RefreshTokenExpiresAt);
        Guard.ThrowIfNull(tokens.AccessTokenExpiresAt);

        // Save refresh token
        var result = await _refreshTokenDb.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = tokens.RefreshToken,
            InvalidAt = tokens.RefreshTokenExpiresAt.Value,
            TokenFamily = tokenFamily
        });

        if (result.IsError)
        {
            return FromError(result);
        }


        // Set refresh token to http only cookie
        Result refreshTokenSet = await WriteRefreshTokenHeader(tokens, HttpContext);

        if (refreshTokenSet.IsError)
        {
            return FromError(refreshTokenSet);
        }


        // Return result
        TokenInfo tokenInfo = new(
            Message: "Refresh token cookie set, access token in body.",
            AccessToken: tokens.AccessToken,
            AccessTokenExpiresAt: tokens.AccessTokenExpiresAt.Value,
            RefreshTokenExpiresAt: tokens.RefreshTokenExpiresAt.Value);

        return Ok(tokenInfo);
    }


    private Task<Result> WriteRefreshTokenHeader(TokenGetDto tokens, HttpContext context)
    {
        Guard.ThrowIfNull(tokens);
        Guard.ThrowIfNull(context);

        // Write header
        string controllerPath = ControllerContext.ActionDescriptor.ControllerName;
        context.Response.Cookies.Append(RefreshTokenCookieName, tokens.RefreshToken,
            new CookieOptions
            {
                MaxAge = _tokenService.RefreshTokenLifetime,
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Path = controllerPath
            });

        return Task.FromResult(Result.Ok);
    }



    private Guid ReadEntraId()
    {
        // TODO: Implement this 
        // Claims reader might also work here, if Guid is in same JWT field
        throw new NotImplementedException();
    }

    private async Task<Result<DbUser>> SetupNewUser(Guid entraId)
    {
        var user = new User
        {
            Id = Guid.Empty,
            EntraId = entraId,
            Permissions = Permissions.None,
            StartYear = null
        };

        Result<Guid> userId = await _userDb.Insert(user);
        if (userId.IsError)
        {
            return (Error)userId;
        }

        DbUser? result = await _userDb.GetByUserId((Guid)userId);
        if (result is null)
        {
            return Error.InternalServerError("Failed to create user.");
        }
        return result;
    }
}

