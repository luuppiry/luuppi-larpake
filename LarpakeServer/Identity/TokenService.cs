using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace LarpakeServer.Identity;

public class TokenService : IClaimsReader
{
    readonly ILogger<TokenService> _logger;
    readonly LarpakeIdOptions _options;
    const string BearerPrefix = "Bearer ";

    public TimeSpan AccessTokenLifetime { get; }
    public TimeSpan RefreshTokenLifetime { get; }

    public TokenService(IOptions<LarpakeIdOptions> options, ILogger<TokenService> logger)
    {
        _options = options.Value;
        _logger = logger;

        AccessTokenLifetime = TimeSpan.FromMinutes(_options.AccessTokenLifetimeMinutes);
        RefreshTokenLifetime = TimeSpan.FromDays(_options.RefreshTokenLifetimeDays);
    }

    public string GenerateToken(User user)
    {
        JwtSecurityTokenHandler tokenHandler = new();


        List<Claim> claims = [
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (Constants.PermissionsFieldName, ((int)user.Permissions).ToString()),
            new (Constants.StartYearFieldName, user.StartYear?.ToString() ?? "null"),
        ];

        Guard.ThrowIfNull(_options.SecretBytes);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(AccessTokenLifetime),
            IssuedAt = DateTime.UtcNow,
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_options.SecretBytes),
                SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        if (_options.RefreshTokenByteLength <= 0)
        {
            throw new InvalidOperationException("Refresh token byte length must be positive.");
        }
        var number = new byte[_options.RefreshTokenByteLength];
        using var gen = RandomNumberGenerator.Create();
        gen.GetBytes(number);
        return Convert.ToBase64String(number);
    }


    public TokenGetDto GenerateTokens(User user)
    {
        Guard.ThrowIfNull(user);

        string accessToken = GenerateToken(user);
        string refreshToken = GenerateRefreshToken();
        DateTime refreshExpires = DateTime.UtcNow.Add(RefreshTokenLifetime);
        DateTime accessExpires = DateTime.UtcNow.Add(AccessTokenLifetime);

        return new TokenGetDto(accessExpires, refreshExpires)
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
    }


    public bool ValidateAccessToken(string token, [NotNullWhen(true)]out ClaimsPrincipal? claims, bool validateExpiration = true)
    {
        Guard.ThrowIfNull(token);

        // Create token handler and clear default mappings (e.g. "name" -> "some/xmlsoap/string")
        JwtSecurityTokenHandler tokenHandler = new();
        tokenHandler.InboundClaimTypeMap.Clear();

        Guard.ThrowIfNull(_options.SecretBytes);

        // Set validation parameters
        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(_options.SecretBytes),
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateExpiration
        };


        // Token might have "Bearer <token>" format, remove it
        if (token.StartsWith(BearerPrefix))
        {
            token = token[BearerPrefix.Length..];
        }

        // Validate
        try
        {
            claims = tokenHandler.ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch (Exception ex)
        {
            if (ex is SecurityTokenValidationException or ArgumentException)
            {
                _logger.LogInformation("Token invalidated because of: {msg}", ex.Message);
                claims = null;
                return false;
            }
            throw;
        }
    }


    public Guid? GetUserId(ClaimsPrincipal principal)
    {
        Guard.ThrowIfNull(principal);
        
        Claim? idClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
        if (idClaim is null)
        {
            return null;
        }
        _ = Guid.TryParse(idClaim.Value, out Guid id);
        return id;
    }

    public Permissions? GetUserPermissions(ClaimsPrincipal principal)
    {
        Guard.ThrowIfNull(principal);

        Claim? permissionsClaim = principal.FindFirst(Constants.PermissionsFieldName);
        if (permissionsClaim is null)
        {
            return null;
        }
        if (int.TryParse(permissionsClaim.Value, out int permissions) is false)
        {
            return null;
        }
        return (Permissions)permissions;
    }

    public DateTime? GetTokenIssuedAt(ClaimsPrincipal principal)
    {
        Guard.ThrowIfNull(principal);

        Claim? iatClaim = principal.FindFirst(JwtRegisteredClaimNames.Iat);
        if (iatClaim is null)
        {
            return null;
        }
        if (long.TryParse(iatClaim.Value, out long iat) is false)
        {
            return null;
        }
        return DateTimeOffset.FromUnixTimeSeconds(iat).DateTime;
    }

    public int? GetUserStartYear(ClaimsPrincipal principal)
    {
        Guard.ThrowIfNull(principal);

        Claim? syClaim = principal.FindFirst(Constants.StartYearFieldName);
        if (syClaim is null)
        {
            return null;
        }
        if (int.TryParse(syClaim.Value, out int startYear) is false)
        {
            return null;
        }
        return startYear;
    }
}
