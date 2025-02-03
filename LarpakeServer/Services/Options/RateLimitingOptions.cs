namespace LarpakeServer.Services.Options;

public class RateLimitingOptions
{
    public class RateLimiterConfiguration
    {
        public int MaxRequests { get; set; } = -1;
        public int PeriodSeconds { get; set; } = -1;
    }

    public const string SectionName = "RateLimiting";
    public const string GeneralPolicyName = nameof(General);
    public const string AuthPolicyName = nameof(Authentication);

    public required RateLimiterConfiguration General { get; set; }
    public required RateLimiterConfiguration Authentication { get; set; }

}
