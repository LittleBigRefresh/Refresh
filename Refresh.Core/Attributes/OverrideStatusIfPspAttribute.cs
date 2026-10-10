namespace Refresh.Core.RateLimits.EndpointRateLimiting;

[AttributeUsage(AttributeTargets.Method)]
public class OverrideStatusIfPspAttribute : Attribute
{
    public OverrideStatusIfPspAttribute()
    {

    }
}