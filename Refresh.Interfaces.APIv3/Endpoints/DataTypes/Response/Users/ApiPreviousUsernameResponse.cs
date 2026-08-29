using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiPreviousUsernameResponse : IApiResponse, IDataConvertableFrom<ApiPreviousUsernameResponse, PreviousUsername>
{
    public required string Username { get; set; }
    public required ApiGameUserResponse User { get; set; }
    public required DateTimeOffset ReplacedAt { get; set; }
    
    public static ApiPreviousUsernameResponse? FromOld(PreviousUsername? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiPreviousUsernameResponse
        {
            Username = old.Username,
            User = ApiGameUserResponse.FromOld(old.User, dataContext)!,
            ReplacedAt = old.ReplacedAt,
        };
    }

    public static IEnumerable<ApiPreviousUsernameResponse> FromOldList(IEnumerable<PreviousUsername> oldList, DataContext dataContext) 
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}

