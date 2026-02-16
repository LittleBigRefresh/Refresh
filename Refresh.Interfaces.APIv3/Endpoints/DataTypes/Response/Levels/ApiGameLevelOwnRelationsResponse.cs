using Refresh.Core.Types.Data;
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

        return new()
        {
            // TODO: Probably cache these stats aswell
            IsHearted = dataContext.Database.IsLevelFavouritedByUser(level, dataContext.User),
            IsQueued = dataContext.Database.IsLevelQueuedByUser(level, dataContext.User),
            LevelRating = (int?)dataContext.Database.GetRatingByUser(level, dataContext.User) ?? 0,
            MyPlaysCount = dataContext.Database.GetTotalPlaysForLevelByUser(level, dataContext.User),
            CompletionCount = dataContext.Database.GetTotalCompletionsForLevelByUser(level, dataContext.User),
            PhotoCount = dataContext.Database.GetTotalPhotosInLevelByUser(level, dataContext.User)
        };
    }
}
