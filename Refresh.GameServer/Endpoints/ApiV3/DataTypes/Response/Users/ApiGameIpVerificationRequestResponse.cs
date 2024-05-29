using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameIpVerificationRequestResponse : IApiResponse, IDataConvertableFrom<ApiGameIpVerificationRequestResponse, GameIpVerificationRequest>
{
    public required string IpAddress { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    
    public static ApiGameIpVerificationRequestResponse? FromOld(GameIpVerificationRequest? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiGameIpVerificationRequestResponse
        {
            IpAddress = old.IpAddress,
            CreatedAt = old.CreatedAt,
        };
    }

    public static IEnumerable<ApiGameIpVerificationRequestResponse> FromOldList(
        IEnumerable<GameIpVerificationRequest> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}