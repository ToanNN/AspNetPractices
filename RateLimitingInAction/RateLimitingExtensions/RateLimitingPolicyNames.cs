﻿namespace RateLimitingInAction.RateLimitingExtensions;

public static class RateLimitingPolicyNames
{
    public const string Fixed = "Fixed";
    public const string Sliding = "Sliding";
    public const string TokenBucket = "TokenBucket";
    public const string Concurrency = "Concurrency";
    public const string PerUser = "PerUser";
    public const string Jwt = "Jwt";
}