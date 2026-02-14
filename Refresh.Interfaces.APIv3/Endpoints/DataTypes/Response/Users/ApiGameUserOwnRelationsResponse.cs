using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameUserOwnRelationsResponse : IApiResponse
{
    public required bool IsHearted { get; set; }

    public static ApiGameUserOwnRelationsResponse? FromOld(GameUser user, DataContext dataContext)
    {
        if (dataContext.User == null) 
            return null;

        return new()
        {
            IsHearted = dataContext.Database.IsUserFavouritedByUser(user, dataContext.User),
        };
    }
}