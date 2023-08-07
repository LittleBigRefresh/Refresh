using JetBrains.Annotations;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

/// <summary>
/// A user with full information, like current role, ban status, etc.
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiExtendedGameUserResponse : IApiResponse, IDataConvertableFrom<ApiExtendedGameUserResponse, GameUser>
{
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public required string IconHash { get; set; }
    public required string Description { get; set; }
    public required ApiGameLocationResponse Location { get; set; }
    public required DateTimeOffset JoinDate { get; set; }
    
    public required bool AllowIpAuthentication { get; set; }
    public required GameUserRole Role { get; set; }
    
    public required string? BanReason { get; set; }
    public required DateTimeOffset? BanExpiryDate { get; set; }
    
    [ContractAnnotation("null => null; notnull => notnull")]
    public static ApiExtendedGameUserResponse? FromOld(GameUser? user)
    {
        if (user == null) return null;
        
        return new ApiExtendedGameUserResponse
        {
            UserId = user.UserId.ToString()!,
            Username = user.Username,
            IconHash = user.IconHash,
            Description = user.Description,
            Location = ApiGameLocationResponse.FromGameLocation(user.Location)!,
            JoinDate = user.JoinDate,
            AllowIpAuthentication = user.AllowIpAuthentication,
            Role = user.Role,
            BanReason = user.BanReason,
            BanExpiryDate = user.BanExpiryDate,
        };
    }

    public static IEnumerable<ApiExtendedGameUserResponse> FromOldList(IEnumerable<GameUser> oldList) => oldList.Select(FromOld)!;
}