using Refresh.Core.Types.Data;
using Refresh.Database.Models.Relations;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameUserVerifiedIpResponse : IApiResponse, IDataConvertableFrom<ApiGameUserVerifiedIpResponse, GameUserVerifiedIpRelation>
{
    public required string IpAddress { get; set; }
    public required DateTimeOffset VerifiedAt { get; set; }
    
    public static ApiGameUserVerifiedIpResponse? FromOld(GameUserVerifiedIpRelation? old, DataContext dataContext)
    {
        if (old == null)
            return null;

        return new ApiGameUserVerifiedIpResponse
        {
            IpAddress = old.IpAddress,
            VerifiedAt = old.VerifiedAt,
        };
    }

    public static IEnumerable<ApiGameUserVerifiedIpResponse> FromOldList(IEnumerable<GameUserVerifiedIpRelation> oldList, DataContext dataContext) 
        => oldList.Select(r => FromOld(r, dataContext)!);
}