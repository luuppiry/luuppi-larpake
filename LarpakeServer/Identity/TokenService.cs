using LarpakeServer.Models.DatabaseModels;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace LarpakeServer.Identity;

public class TokenService : IClaimsReader
{
    readonly IConfiguration _config;
    readonly ILogger<TokenService> _logger;
    readonly TimeSpan _accessTokenLifetimeMinutes;
    readonly TimeSpan _refreshTokenLifetimeDays;


    public TokenService(IConfiguration config, ILogger<TokenService> logger)
    {
        _config = config;
        _logger = logger;

        _accessTokenLifetimeMinutes = TimeSpan.FromMinutes(
            _config.GetValue<int>("Jwt:TokenLifetimeMinutes"));

        _refreshTokenLifetimeDays = TimeSpan.FromDays(
            _config.GetValue<int>("Jwt:RefreshTokenLifetimeDays"));
    }

    public string GenerateToken(User user)
    {
        JwtSecurityTokenHandler tokenHandler = new();

        byte[] key = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!);

        List<Claim> claims = [
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (Constants.PermissionsFieldName, ((int)user.Permissions).ToString()),
            new (Constants.StartYearFieldName, user.StartYear?.ToString() ?? "null"),
        ];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(_accessTokenLifetimeMinutes),
            IssuedAt = DateTime.UtcNow,
            Issuer = _config.GetValue<string>("Jwt:Issuer"),
            Audience = _config.GetValue<string>("Jwt:Audience"),

            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var number = new byte[64];
        using var gen = RandomNumberGenerator.Create();
        gen.GetBytes(number);
        return Convert.ToBase64String(number);
    }


    public TokenDto GenerateTokens(User user)
    {
        Guard.ThrowIfNull(user);

        string accessToken = GenerateToken(user);
        string refreshToken = GenerateRefreshToken();


        throw new NotImplementedException();
    }


    public bool ValidateAccessToken(string token, [NotNullWhen(true)]out ClaimsPrincipal? claims, bool validateExpiration = true)
    {
        Guard.ThrowIfNull(token);

        JwtSecurityTokenHandler tokenHandler = new();
        byte[] key = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!);

        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidIssuer = _config["Jwt:Issuer"],
            ValidAudience = _config["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateExpiration,
        };
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
}
