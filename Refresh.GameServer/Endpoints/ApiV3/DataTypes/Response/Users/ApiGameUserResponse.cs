using JetBrains.Annotations;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Data;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users.Rooms;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameUserResponse : IApiResponse, IDataConvertableFrom<ApiGameUserResponse, GameUser>
{
    // HEY! When adding fields here, remember to propagate them in ApiExtendedGameUser too!
    // Otherwise, they won't show up in the admin panel endpoints or /users/me. Thank you!
    
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public required string IconHash { get; set; }

    public required string YayFaceHash { get; set; }
    public required string BooFaceHash { get; set; }
    public required string MehFaceHash { get; set; }
    public required string Description { get; set; }
    public required ApiGameLocationResponse Location { get; set; }
    public required DateTimeOffset JoinDate { get; set; }
    public required DateTimeOffset LastLoginDate { get; set; }
    public required GameUserRole Role { get; set; }
    
    public required ApiGameUserStatisticsResponse Statistics { get; set; }
    public required ApiGameRoomResponse? ActiveRoom { get; set; }

    [ContractAnnotation("null => null; notnull => notnull")]
    public static ApiGameUserResponse? FromOld(GameUser? user, DataContext dataContext)
    {
        if (user == null) return null;
        
        return new ApiGameUserResponse
        {
            UserId = user.UserId.ToString()!,
            Username = user.Username,

            IconHash = dataContext.GetIconFromHash(user.IconHash),
            YayFaceHash = dataContext.GetIconFromHash(user.YayFaceHash),
            BooFaceHash = dataContext.GetIconFromHash(user.BooFaceHash),
            MehFaceHash = dataContext.GetIconFromHash(user.MehFaceHash),

            Description = user.Description,
            Location = ApiGameLocationResponse.FromLocation(user.LocationX, user.LocationY)!,
            JoinDate = user.JoinDate,
            LastLoginDate = user.LastLoginDate,
            Role = user.Role,
            Statistics = ApiGameUserStatisticsResponse.FromOld(user, dataContext)!,
            ActiveRoom = ApiGameRoomResponse.FromOld(dataContext.Match.RoomAccessor.GetRoomByUser(user), dataContext),
        };
    }

    public static IEnumerable<ApiGameUserResponse> FromOldList(IEnumerable<GameUser> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}