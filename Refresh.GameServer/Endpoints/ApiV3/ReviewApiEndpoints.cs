using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Refresh.Database;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Levels;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Reviews;
using GameDatabaseContext = Refresh.Database.GameDatabaseContext;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class ReviewApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("levels/id/{id}/reviews"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets a list of the reviews posted to a level.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiListResponse<ApiGameReviewResponse> GetTopScoresForLevel(RequestContext context,
        GameDatabaseContext database, IDataStore dataStore,
        [DocSummary("The ID of the level")] int id, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        (int skip, int count) = context.GetPageData();
        
        DatabaseList<GameReview> reviews = database.GetReviewsForLevel(level, count, skip);
        DatabaseList<ApiGameReviewResponse> ret = DatabaseListExtensions.FromOldList<ApiGameReviewResponse, GameReview>(reviews, dataContext);

        return ret;
    }
}