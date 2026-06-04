using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Disallowed;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiDisallowedUsernameResponse : IApiResponse, IDataConvertableFrom<ApiDisallowedUsernameResponse, DisallowedUser>
{
    public required string Username { get; set; }
    public required string Reason { get; set; }
    public required DateTimeOffset DisallowedAt { get; set; }
    
    public static ApiDisallowedUsernameResponse? FromOld(DisallowedUser? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiDisallowedUsernameResponse
        {
            Username = old.Username,
            Reason = old.Reason,
            DisallowedAt = old.DisallowedAt,
        };
    }

    public static IEnumerable<ApiDisallowedUsernameResponse> FromOldList(IEnumerable<DisallowedUser> oldList, DataContext dataContext)
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}