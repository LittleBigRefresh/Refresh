using Refresh.Core.Configuration;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiEntityRateLimitResponse : IApiResponse
{
    public required int TimeSpanHours { get; set; }
    public required int EntityQuota { get; set; }

    public static ApiEntityRateLimitResponse? FromOld(EntityUploadRateLimitProperties old)
    {
        if (!old.Enabled) return null;

        return new()
        {
            TimeSpanHours = old.TimeSpanHours,
            EntityQuota = old.EntityQuota,
        };
    }
}