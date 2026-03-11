using Refresh.Core.Types.Data;
using Refresh.Core.Types.Relations;
using Refresh.Database.Models.Levels;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameLevelOwnRelationsResponse : IApiResponse
{
    public required bool IsHearted { get; set; }
    public required bool IsQueued { get; set; }
    public required int LevelRating { get; set; }

    /// <summary>
    /// Returns the total amount of plays. Probably rename this in APIv4 for clarity.
    /// </summary>
    public required int MyPlaysCount { get; set; }
    public required int CompletionCount { get; set; }
    public required int PhotoCount { get; set; }

    public static ApiGameLevelOwnRelationsResponse? FromOld(GameLevel level, DataContext dataContext)
    {
        if (dataContext.User == null) 
            return null;
        
        OwnLevelRelations relations = dataContext.Cache.GetOwnLevelRelations(dataContext.User, level, dataContext.Database).Content;

        return new()
        {
            IsHearted = relations.IsHearted,
            IsQueued = relations.IsQueued,
            LevelRating = relations.LevelRating,
            MyPlaysCount = relations.TotalPlayCount,
            CompletionCount = relations.TotalCompletionCount,
            PhotoCount = relations.PhotoCount
        };
    }
}
