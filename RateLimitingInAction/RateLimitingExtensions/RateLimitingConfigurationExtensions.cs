using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace RateLimitingInAction.RateLimitingExtensions;

public static class RateLimitingConfigurationExtensions
{
    public static string ConfigureRateLimiting(this WebApplicationBuilder webApplicationBuilder)
    {
        var rateLimitSettings = new RateLimitSettings();
        webApplicationBuilder.Configuration.GetSection(RateLimitSettings.RateLimitSettingsConfigSectionName)
            .Bind(rateLimitSettings);

        //Add Fixed Window Rate Limiter service
        webApplicationBuilder.Services.AddRateLimiter(options =>
            options.AddFixedRateLimiter(RateLimitingPolicyNames.Fixed, rateLimitSettings)
                .AddSlidingRateLimiter(RateLimitingPolicyNames.Sliding, rateLimitSettings)
                .AddTokenRateLimiter(RateLimitingPolicyNames.TokenBucket, rateLimitSettings)
                .AddConcurrencyRateLimiter(RateLimitingPolicyNames.Concurrency, rateLimitSettings)
        );
        return RateLimitingPolicyNames.Fixed;
    }

    private static RateLimiterOptions AddConcurrencyRateLimiter(this RateLimiterOptions options, string policyName,
        RateLimitSettings rateLimitSettings)
    {
        return options.AddConcurrencyLimiter(policyName, concurrencyOptions =>
        {
            concurrencyOptions.PermitLimit = rateLimitSettings.PermitLimit;
            concurrencyOptions.QueueLimit = rateLimitSettings.QueueLimit;
            concurrencyOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        });
    }

    private static RateLimiterOptions AddTokenRateLimiter(this RateLimiterOptions options, string policyName,
        RateLimitSettings rateLimitSettings)
    {
        return options.AddTokenBucketLimiter(policyName, bucketRateOptions =>
        {
            bucketRateOptions.TokenLimit = rateLimitSettings.TokenLimit;
            // True - the token is automatically replenished. False someone has to call TryReplenish()
            bucketRateOptions.AutoReplenishment = rateLimitSettings.AutoReplenishment;

            // How many tokens will be replenished per ReplenishmentPeriod
            bucketRateOptions.ReplenishmentPeriod = rateLimitSettings.ReplenishmentPeriod;
            bucketRateOptions.TokensPerPeriod = rateLimitSettings.TokensPerPeriod;

            bucketRateOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            bucketRateOptions.QueueLimit = rateLimitSettings.QueueLimit;
        });
    }

    private static RateLimiterOptions AddSlidingRateLimiter(this RateLimiterOptions options, string policyName,
        RateLimitSettings rateLimitSettings)
    {
        return options.AddSlidingWindowLimiter(policyName, slidingOptions =>
        {
            slidingOptions.PermitLimit = rateLimitSettings.PermitLimit;
            slidingOptions.Window = rateLimitSettings.Window;
            slidingOptions.SegmentsPerWindow = rateLimitSettings.SegmentsPerWindow;
            slidingOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            slidingOptions.QueueLimit = rateLimitSettings.QueueLimit;
        });
    }

    private static RateLimiterOptions AddFixedRateLimiter(this RateLimiterOptions options, string policyName,
        RateLimitSettings rateLimitSettings)
    {
        return options.AddFixedWindowLimiter(policyName, fixedRateOptions =>
        {
            fixedRateOptions.PermitLimit = rateLimitSettings.PermitLimit;
            fixedRateOptions.Window = rateLimitSettings.Window;
            fixedRateOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            fixedRateOptions.QueueLimit = rateLimitSettings.QueueLimit;
        });
    }
}