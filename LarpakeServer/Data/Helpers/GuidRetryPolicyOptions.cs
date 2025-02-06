namespace LarpakeServer.Data.Helpers;

public class GuidRetryPolicyOptions
{
    public const string SectionName = "GuidRetryPolicy";
    public int MaxRetries { get; set; } = 1;
}
