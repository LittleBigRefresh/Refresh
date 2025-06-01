using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Interfaces.APIv3.Documentation.Attributes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;
using Refresh.Interfaces.APIv3.Extensions;

namespace Refresh.Interfaces.APIv3.Endpoints;

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