using System.Globalization;
using System.Net;
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
            {
                options.AddFixedRateLimiter(RateLimitingPolicyNames.Fixed, rateLimitSettings)
                    .AddSlidingRateLimiter(RateLimitingPolicyNames.Sliding, rateLimitSettings)
                    .AddTokenRateLimiter(RateLimitingPolicyNames.TokenBucket, rateLimitSettings)
                    .AddConcurrencyRateLimiter(RateLimitingPolicyNames.Concurrency, rateLimitSettings);

                options.OnRejected = HandleOverLimitRequests;

                options.AddRateLimiterPerUser(RateLimitingPolicyNames.PerUser, rateLimitSettings);

                //Vulnerable to DOS
                options.AddGlobalRateLimiter(rateLimitSettings);
            }
        );
        return RateLimitingPolicyNames.Fixed;
    }

    private static RateLimiterOptions AddGlobalRateLimiter(this RateLimiterOptions options,
        RateLimitSettings rateLimitSettings)
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
        {
            IPAddress? remoteIpAddress = context.Connection.RemoteIpAddress;
            if (!IPAddress.IsLoopback(remoteIpAddress!))
            {
                return RateLimitPartition.GetTokenBucketLimiter(remoteIpAddress!, _ =>
                    new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = rateLimitSettings.TokenLimit2,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = rateLimitSettings.QueueLimit,
                        ReplenishmentPeriod = rateLimitSettings.ReplenishmentPeriod,
                        TokensPerPeriod = rateLimitSettings.TokensPerPeriod,
                        AutoReplenishment = rateLimitSettings.AutoReplenishment
                    });
            }

            return RateLimitPartition.GetNoLimiter(IPAddress.Loopback);
        });
        return options;
    }

    private static RateLimiterOptions AddRateLimiterPerUser(this RateLimiterOptions options, string policyName,
        RateLimitSettings rateLimitSettings)
    {
        //Add a new rate limiting policy
        options.AddPolicy(policyName, context =>
        {
            var userName = "Anonymous";
            if (context.User.Identity?.IsAuthenticated is true)
            {
                userName = context.User.ToString()!;
            }

            return RateLimitPartition.GetSlidingWindowLimiter(userName, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = rateLimitSettings.PermitLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                Window = rateLimitSettings.Window,
                SegmentsPerWindow = rateLimitSettings.SegmentsPerWindow
            });
        });
        return options;
    }


    private static ValueTask HandleOverLimitRequests(OnRejectedContext context, CancellationToken cancellationToken)
    {
        // Has retry header
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        context.HttpContext.RequestServices.GetService<ILoggerFactory>()?
            .CreateLogger("Microsoft.AspNetCore.RateLimitingMiddleware")
            .LogWarning("OnRejected: {GetUserEndPoint}", GetUserEndPoint(context.HttpContext));


        return ValueTask.CompletedTask;
    }

    private static string GetUserEndPoint(HttpContext context)
    {
        return
            $"User {context.User.Identity?.Name ?? "Anonymous"} endpoint: {context.Request.Path} {context.Connection.RemoteIpAddress} ";
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