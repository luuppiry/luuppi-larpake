using LarpakeServer.Data;
using LarpakeServer.Extensions;
using LarpakeServer.Identity;
using LarpakeServer.Identity.EntraId;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Services.Options;
using Microsoft.AspNetCore.RateLimiting;
using DbUser = LarpakeServer.Models.DatabaseModels.User;

namespace LarpakeServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(RateLimitingOptions.AuthPolicyName)]
public class AuthenticationController : ExtendedControllerBase
{
    readonly TokenService _tokenService;
    readonly EntraTokenReader _entraTokenReader;
    readonly IUserDatabase _userDb;
    readonly IRefreshTokenDatabase _refreshTokenDb;


    const string RefreshTokenCookieName = "__Secure-refreshToken";

    enum LoginAction
    {
        Undefined,
        Login,
        Refresh,
        Register
    }
    record TokenInfo(string Message, string AccessToken, DateTime AccessTokenExpiresAt, DateTime RefreshTokenExpiresAt, string TokenType = "Bearer", string Action = "None");


    public AuthenticationController(
        TokenService generator,
        EntraTokenReader entraTokenReader,
        IUserDatabase userDb,
        IRefreshTokenDatabase refreshTokenDb,
        IClaimsReader claimsReader,
        ILogger<AuthenticationController> logger) : base(claimsReader, logger)
    {
        _tokenService = generator;
        _entraTokenReader = entraTokenReader;
        _userDb = userDb;
        _refreshTokenDb = refreshTokenDb;
    }

    /* Notice that normally Constants.Auth.LarpakeIdScheme
     * is used when the user is authenticated. This endpoint
     * needs to validate Entra id access token, so different 
     * scheme is used. After this login call, other endpoints
     * should use the normal LarpakeIdScheme.
     */
    [Authorize(AuthenticationSchemes = Constants.Auth.EntraIdScheme)]
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenInfo), 200)]
    [ProducesErrorResponseType(typeof(MessageResponse))]
    public async Task<IActionResult> Login()
    {
        // Validate user id
        Guid entraId = ReadEntraId();
        if (entraId == Guid.Empty)
        {
            return BadRequest("Token must provide user id (oid).");
        }

        // Check if user exists in database
        DbUser? user = await _userDb.GetByEntraId(entraId);
        if (user is null)
        {
            // Validate email/username
            string email = ReadEntraUsername();
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Token must contain email address or username.");
            }
            if (email.Length > Constants.MaxUsernameLength)
            {
                return BadRequest("Token email/username is too long.");
            }

            // User is new and must be created first
            Result<DbUser> createdUser = await SetupNewUser(entraId, email);
            if (createdUser.IsError)
            {
                return FromError(createdUser);
            }

            _logger.LogInformation("Created user {userId} with entra id {entraId}.", ((DbUser)createdUser).Id, entraId);

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
    [ProducesResponseType(typeof(TokenInfo), 200)]
    [ProducesErrorResponseType(typeof(MessageResponse))]
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
        return await SaveToken(user, tokens, Guid.Empty, LoginAction.Login);
    }
#endif

    [AllowAnonymous]    // Authentication is handled inside the method, Anonymous is ok here.
    [HttpGet("token/refresh")]
    [ProducesResponseType(typeof(TokenInfo), 200)]
    [ProducesErrorResponseType(typeof(MessageResponse))]
    public async Task<IActionResult> Refresh()
    {
        // Read headers and cookies to get tokens
        if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out string? refreshToken) is false)
        {
            return Unauthorized();
        }

        // Get tokens and validate not empty

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized();
        }

        // Validate refresh token
        RefreshTokenValidationResult validation = await _refreshTokenDb.Validate(refreshToken);
        if (validation.IsValid is false)
        {
            return Unauthorized();
        }

        // Tokens are valid, generate new ones
        DbUser? user = await _userDb.GetByUserId(validation.UserId.Value);
        if (user is null)
        {
            return IdNotFound("User does not exist.");
        }

        // Generate new tokens
        TokenGetDto newTokens = _tokenService.GenerateTokens(user);

        _logger.LogInformation("Refreshed tokens for user {id}.", user.Id);

        // Finish by saving refresh token
        return await SaveToken(user, newTokens, validation.TokenFamily.Value, LoginAction.Refresh);
    }


    [Authorize]
    [DisableRateLimiting]
    [HttpPost("token/invalidate")]
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
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
    [ProducesResponseType(typeof(RowsAffectedResponse), 200)]
    public async Task<IActionResult> InvalidateTokenFamily(Guid tokenFamily)
    {
        int rowsAffected = await _refreshTokenDb.RevokeFamily(tokenFamily);
        return OkRowsAffected(rowsAffected);
    }


    private async Task<IActionResult> SaveToken(DbUser user, TokenGetDto tokens,
        Guid tokenFamily, LoginAction action = LoginAction.Undefined)
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
            RefreshTokenExpiresAt: tokens.RefreshTokenExpiresAt.Value,
            Action: action switch
            {
                LoginAction.Refresh => "Refresh",
                LoginAction.Login => "Login",
                LoginAction.Register => "Register",
                _ => "Undefined"
            });

        return Ok(tokenInfo);
    }


    private Task<Result> WriteRefreshTokenHeader(TokenGetDto tokens, HttpContext context)
    {
        Guard.ThrowIfNull(tokens);
        Guard.ThrowIfNull(context);

        context.Response.Headers.AccessControlAllowCredentials = "true";
        

        // Write header
        context.Response.Cookies.Append(RefreshTokenCookieName, tokens.RefreshToken,
            new CookieOptions
            {
                MaxAge = _tokenService.RefreshTokenLifetime,

                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None
            });

        return Task.FromResult(Result.Ok);
    }


    private Guid ReadEntraId()
    {
        Guid? userId = _entraTokenReader.GetUserId(HttpContext.User);
        Guard.ThrowIfNull(userId);
        return userId.Value;
    }

    private string ReadEntraUsername()
    {
        string? email = _entraTokenReader.GetUsername(HttpContext.User);
        Guard.ThrowIfNull(email);
        return email;
    }

    private async Task<Result<DbUser>> SetupNewUser(Guid entraId, string email)
    {
        var user = new DbUser
        {
            Id = Guid.Empty,
            EntraId = entraId,
            Permissions = Permissions.None,
            StartYear = null,
            EntraUsername = email
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

