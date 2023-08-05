using JetBrains.Annotations;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiOwnGameUserResponse : IApiResponse, IDataConvertableFrom<ApiOwnGameUserResponse, GameUser>
{
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public required string IconHash { get; set; }
    public required string Description { get; set; }
    public required ApiGameLocationResponse Location { get; set; }
    public required DateTimeOffset JoinDate { get; set; }
    
    public required bool AllowIpAuthentication { get; set; }
    public required GameUserRole Role { get; set; }
    
    [ContractAnnotation("null => null; notnull => notnull")]
    public static ApiOwnGameUserResponse? FromOld(GameUser? user)
    {
        if (user == null) return null;
        
        return new ApiOwnGameUserResponse
        {
            UserId = user.UserId.ToString()!,
            Username = user.Username,
            IconHash = user.IconHash,
            Description = user.Description,
            Location = ApiGameLocationResponse.FromGameLocation(user.Location)!,
            JoinDate = DateTimeOffset.FromUnixTimeMilliseconds(user.JoinDate),
            AllowIpAuthentication = user.AllowIpAuthentication,
            Role = user.Role,
        };
    }

    public static IEnumerable<ApiOwnGameUserResponse> FromOldList(IEnumerable<GameUser> oldList) => oldList.Select(FromOld)!;
}