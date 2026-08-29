using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiExtendedPreviousUsernameResponse : ApiPreviousUsernameResponse, IDataConvertableFrom<ApiExtendedPreviousUsernameResponse, PreviousUsername>
{
    public new static ApiExtendedPreviousUsernameResponse? FromOld(PreviousUsername? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiExtendedPreviousUsernameResponse
        {
            Username = old.Username,
            User = ApiExtendedGameUserResponse.FromOld(old.User, dataContext)!,
            ReplacedAt = old.ReplacedAt,
        };
    }

    public new static IEnumerable<ApiExtendedPreviousUsernameResponse> FromOldList(IEnumerable<PreviousUsername> oldList, DataContext dataContext) 
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}