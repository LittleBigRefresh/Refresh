using Refresh.Core.Types.Data;
using Refresh.Core.Types.Relations;
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
        
        OwnUserRelations relations = dataContext.Cache.GetOwnUserRelations(dataContext.User, user, dataContext.Database).Content;

        return new()
        {
            IsHearted = relations.IsHearted,
        };
    }
}