using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Endpoints.Debugging;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class ReviewEndpoints : EndpointGroup
{
    [GameEndpoint("dpadrate/{type}/{levelId}", HttpMethods.Post)]
    public Response SubmitRating(RequestContext context, IGameDatabaseContext database, GameUser user, int levelId)
    {
        GameLevel? level = database.GetLevelById(levelId);
        if (level == null) return NotFound;

        bool parsed = sbyte.TryParse(context.QueryString.Get("rating"), out sbyte rating);
        if (!parsed) return BadRequest;

        //dpad ratings can only be -1 0 1
        if (rating is > 1 or < -1) return BadRequest;
        
        bool rated = database.RateLevel(level, user, (RatingType)rating);
        return rated ? OK : Unauthorized;
    }
    
    [GameEndpoint("rate/user/{levelId}", ContentType.Xml, HttpMethods.Post)]
    [AllowEmptyBody]
    public Response RateUserLevel(RequestContext context, IGameDatabaseContext database, GameUser user, int levelId)
    {
        GameLevel? level = database.GetLevelById(levelId);
        if (level == null) return NotFound;

        if (!int.TryParse(context.QueryString.Get("rating"), out int ratingInt)) return BadRequest;
        
        RatingType rating;
        switch (ratingInt)
        {
            case 1:
            case 2:
                rating = RatingType.Boo;
                break;
            case 3:
                rating = RatingType.Neutral;
                break;
            case 4:
            case 5:
                rating = RatingType.Yay;
                break;
            default:
                return BadRequest;
        }

        return database.RateLevel(level, user, rating) ? OK : Unauthorized;
    }

    [GameEndpoint("reviewsFor/{slotType}/{levelId}", ContentType.Xml)]
    [AllowEmptyBody]
    public Response GetReviewsForLevel(RequestContext context, IGameDatabaseContext database, string slotType, int levelId)
    {
        GameLevel? level;
        switch (slotType)
        {
            case "developer":
                level = database.GetStoryLevelById(levelId);
                break;
            case "user":
                level = database.GetLevelById(levelId);
                break;
            default:
                return BadRequest;
        }

        if (level == null) 
            return NotFound;

        (int skip, int count) =  context.GetPageData();
        
        return new Response(new SerializedGameReviewResponse(items: SerializedGameReview.FromOldList(database.GetReviewsForLevel(level, count, skip).Items).ToList()), ContentType.Xml);
    }
    
    [GameEndpoint("reviewsBy/{username}", ContentType.Xml)]
    [AllowEmptyBody]
    public Response GetReviewsForLevel(RequestContext context, IGameDatabaseContext database, string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        
        if (user == null) 
            return NotFound;

        (int skip, int count) =  context.GetPageData();
        
        return new Response(new SerializedGameReviewResponse(SerializedGameReview.FromOldList(database.GetReviewsByUser(user, count, skip).Items).ToList()), ContentType.Xml);
    }

    [GameEndpoint("postReview/{slotType}/{levelId}", ContentType.Xml, HttpMethods.Post)]
    public Response PostReviewForLevel(
        RequestContext context,
        IGameDatabaseContext database,
        string slotType,
        int levelId,
        SerializedGameReview body,
        GameUser user,
        IDateTimeProvider timeProvider
    )
    {
        GameLevel? level;
        switch (slotType)
        {
            case "developer":
                level = database.GetStoryLevelById(levelId);
                break;
            case "user":
                level = database.GetLevelById(levelId);
                break;
            default:
                return BadRequest;
        }

        if (level == null)
            return NotFound;

        //You cant review a level you haven't played.
        if (!database.HasUserPlayedLevel(level, user))
            return BadRequest;

        //Add the review to the database
        database.AddReviewToLevel(new GameReview
        {
            Publisher = user,
            Level = level,
            PostedAt = timeProvider.Now,
            Labels = body.Labels,
            Content = body.Text,
        }, level);

        return OK;
    }
}