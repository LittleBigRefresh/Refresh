using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameIpVerificationRequestResponse : IApiResponse
{
    public required string IpAddress { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    
    public static ApiGameIpVerificationRequestResponse? FromOld(GameIpVerificationRequest? old)
    {
        if (old == null) return null;

        return new ApiGameIpVerificationRequestResponse
        {
            IpAddress = old.IpAddress,
            CreatedAt = old.CreatedAt,
        };
    }

    public static IEnumerable<ApiGameIpVerificationRequestResponse> FromOldList(IEnumerable<GameIpVerificationRequest> oldList) => oldList.Select(FromOld).ToList()!;
}