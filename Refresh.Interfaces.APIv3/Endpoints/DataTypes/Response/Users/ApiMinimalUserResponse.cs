using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiMinimalUserResponse : IApiResponse, IDataConvertableFrom<ApiMinimalUserResponse, GameUser>
{
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public required string IconHash { get; set; }

    
    public static ApiMinimalUserResponse? FromOld(GameUser? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiMinimalUserResponse
        {
            UserId = old.UserId.ToString(),
            Username = old.Username,
            IconHash = old.IconHash,
        };
    }

    public static IEnumerable<ApiMinimalUserResponse> FromOldList(IEnumerable<GameUser> oldList, DataContext dataContext) 
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}