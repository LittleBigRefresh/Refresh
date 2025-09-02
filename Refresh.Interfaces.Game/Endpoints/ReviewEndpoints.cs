using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common.Time;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Reviews;

namespace Refresh.Interfaces.Game.Endpoints;

public class ReviewEndpoints : EndpointGroup
{
    [GameEndpoint("dpadrate/{slotType}/{id}", HttpMethods.Post)]
    [RequireEmailVerified]
    public Response SubmitRating(RequestContext context, GameDatabaseContext database, GameUser user, string slotType,
        int id, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;

        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;

        bool parsed = sbyte.TryParse(context.QueryString.Get("rating"), out sbyte rating);
        if (!parsed) return BadRequest;

        //dpad ratings can only be -1 0 1
        if (rating is > 1 or < -1) return BadRequest;
        
        bool rated = database.RateLevel(level, user, (RatingType)rating);
        return rated ? OK : Unauthorized;
    }
    
    [GameEndpoint("rate/{slotType}/{id}", ContentType.Xml, HttpMethods.Post)]
    [AllowEmptyBody]
    public Response RateUserLevel(RequestContext context, GameDatabaseContext database, GameUser user, string slotType, int id)
    {
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
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

    [GameEndpoint("reviewsFor/{slotType}/{id}", ContentType.Xml)]
    [AllowEmptyBody]
    public Response GetReviewsForLevel(RequestContext context, GameDatabaseContext database, string slotType, int id,
        DataContext dataContext)
    {
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);

        if (level == null) 
            return NotFound;

        (int skip, int count) =  context.GetPageData();
        
        return new Response(new SerializedGameReviewResponse(items: SerializedGameReview.FromOldList(database.GetReviewsForLevel(level, count, skip).Items, dataContext).ToList()), ContentType.Xml);
    }
    
    [GameEndpoint("reviewsBy/{username}", ContentType.Xml)]
    [AllowEmptyBody]
    public Response GetReviewsByUser(RequestContext context, GameDatabaseContext database, string username,
        DataContext dataContext)
    {
        GameUser? user = database.GetUserByUsername(username);
        
        if (user == null) 
            return NotFound;

        (int skip, int count) =  context.GetPageData();

        IEnumerable<GameReview> items = database.GetReviewsByUser(user, count, skip)
            .Items
            .ToArrayIfPostgres();

        return new Response(new SerializedGameReviewResponse(SerializedGameReview.FromOldList(items, dataContext).ToList()), ContentType.Xml);
    }

    [GameEndpoint("postReview/{slotType}/{id}", ContentType.Xml, HttpMethods.Post)]
    [RequireEmailVerified]
    public Response PostReviewForLevel(RequestContext context,
        GameDatabaseContext database,
        string slotType,
        int id,
        SerializedGameReview body,
        GameUser user,
        IDateTimeProvider timeProvider,
        GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);

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
            Labels = LabelExtensions.FromLbpCommaList(body.Labels).ToList(),
            Content = body.Text,
        }, level);

        // Update the user's rating
        database.RateLevel(level, user, (RatingType)body.Thumb);

        return OK;
    }
    
    [GameEndpoint("rateReview/user/{levelId}/{username}", HttpMethods.Post)]
    [RequireEmailVerified]
    public Response SubmitReviewRating(RequestContext request, GameDatabaseContext database, GameUser user, int levelId,
        string username, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;
        
        GameUser? reviewer = database.GetUserByUsername(username);
        GameLevel? reviewedLevel = database.GetLevelById(levelId);
        
        if (reviewer == null) return BadRequest;
        if (reviewedLevel == null) return BadRequest;
        
        GameReview? review = database.GetReviewByUserForLevel(reviewer, reviewedLevel);

        if (review == null) return NotFound;
        
        string ratingStr = request.QueryString.Get("rating") ?? "0";
        RatingType ratingType = (RatingType)sbyte.Parse(ratingStr);
        
        // rating can only be 1, 0 or -1
        if(!Enum.IsDefined(ratingType))
        {
            return BadRequest;
        }
        
        if (database.ReviewRatingExists(user, review, ratingType))
        {
            return BadRequest;
        }
        
        database.RateReview(review, ratingType, user);

        return OK;
    }
}