using JetBrains.Annotations;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameUserResponse : IApiResponse, IDataConvertableFrom<ApiGameUserResponse, GameUser>
{
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public required string IconHash { get; set; }
    public required string Description { get; set; }
    public required ApiGameLocationResponse Location { get; set; }

    [ContractAnnotation("null => null; notnull => notnull")]
    public static ApiGameUserResponse? FromOld(GameUser? user)
    {
        if (user == null) return null;
        
        return new ApiGameUserResponse
        {
            UserId = user.UserId.ToString()!,
            Username = user.Username,
            IconHash = user.IconHash,
            Description = user.Description,
            Location = ApiGameLocationResponse.FromGameLocation(user.Location)!,
        };
    }

    public static IEnumerable<ApiGameUserResponse> FromOldList(IEnumerable<GameUser> oldList) => oldList.Select(FromOld)!;
}