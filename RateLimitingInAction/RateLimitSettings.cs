namespace RateLimitingInAction;

public class RateLimitSettings
{
    public const string RateLimitSettingsConfigSectionName = "RateLimitSettings";
    public int PermitLimit { get; set; } = 100;
    public TimeSpan Window { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan ReplenishmentPeriod { get; set; } = TimeSpan.FromSeconds(2);
    public int QueueLimit { get; set; } = 2;
    public int SegmentsPerWindow { get; set; } = 8;
    public int TokenLimit { get; set; } = 10;
    public int BucketTokenLimit { get; set; } = 20;
    public int TokensPerPeriod { get; set; } = 4;
    public bool AutoReplenishment { get; set; } = false;
}