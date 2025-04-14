namespace LarpakeServer.Identity;

public class LarpakeIdOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = Constants.Luuppi.Issuer;
    public string Audience { get; set; } = Constants.Luuppi.Audience;

    public byte[]? SecretBytes { get; private set; }
    public string? SecretKey
    {
        get => field;
        set
        {
            if (value is not null)
            {
                SecretBytes = Encoding.UTF8.GetBytes(value);
            }
            field = value;
        }
    }
    public int RefreshTokenByteLength { get; set; }
    public int AccessTokenLifetimeMinutes { get; set; }
    public int RefreshTokenLifetimeDays { get; set; }
    public uint RefreshTokenExpirationCooldownSeconds { get; set; } = 0;

    internal void LogValues(ILogger logger)
    {
        logger.LogDebug("""
               Id options:
                  - Issuer: {Issuer}
                  - Audience: {Audience}
                  - SecretKey: {(SecretKey is null ? "not found" : "found")}
                  - RefreshTokenByteLength: {RefreshTokenByteLength}
                  - AccessTokenLifetimeMinutes: {AccessTokenLifetimeMinutes}
                  - RefreshTokenLifetimeDays: {RefreshTokenLifetimeDays}
                  - RefreshTokenExpirationCooldownMinutes: {RefreshTokenExpirationCooldownMinutes}
               """,
               Issuer,
               Audience,
               SecretKey is null ? "not found" : "found",
               RefreshTokenByteLength,
               AccessTokenLifetimeMinutes,
               RefreshTokenLifetimeDays,
               RefreshTokenExpirationCooldownSeconds);
    }
}
