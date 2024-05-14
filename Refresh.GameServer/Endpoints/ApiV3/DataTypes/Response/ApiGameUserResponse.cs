using Bunkum.Core.Storage;
using JetBrains.Annotations;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameUserResponse : IApiResponse, IDataConvertableFrom<ApiGameUserResponse, GameUser>
{
    // HEY! When adding fields here, remember to propagate them in ApiExtendedGameUser too!
    // Otherwise, they won't show up in the admin panel endpoints or /users/me. Thank you!
    
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public required string IconHash { get; set; }
    public required string Description { get; set; }
    public required ApiGameLocationResponse Location { get; set; }
    public required DateTimeOffset JoinDate { get; set; }
    public required DateTimeOffset LastLoginDate { get; set; }

    [ContractAnnotation("null => null; notnull => notnull")]
    public static ApiGameUserResponse? FromOld(GameUser? user, DataContext dataContext)
    {
        if (user == null) return null;
        
        return new ApiGameUserResponse
        {
            UserId = user.UserId.ToString()!,
            Username = user.Username,
            IconHash = dataContext.Database.GetAssetFromHash(user.IconHash)?.GetAsIcon(TokenGame.Website, dataContext) ?? user.IconHash,
            Description = user.Description,
            Location = ApiGameLocationResponse.FromGameLocation(user.Location)!,
            JoinDate = user.JoinDate,
            LastLoginDate = user.LastLoginDate,
        };
    }

    public static IEnumerable<ApiGameUserResponse> FromOldList(IEnumerable<GameUser> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}