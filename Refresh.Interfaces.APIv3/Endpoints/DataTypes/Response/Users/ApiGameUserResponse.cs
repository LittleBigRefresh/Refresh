using JetBrains.Annotations;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Data;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users.Rooms;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameUserResponse : IApiResponse, IDataConvertableFrom<ApiGameUserResponse, GameUser>
{
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public required string IconHash { get; set; }
    public required string VitaIconHash { get; set; }
    public required string BetaIconHash { get; set; }

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
            VitaIconHash = dataContext.GetIconFromHash(user.VitaIconHash),
            BetaIconHash = dataContext.GetIconFromHash(user.BetaIconHash),
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