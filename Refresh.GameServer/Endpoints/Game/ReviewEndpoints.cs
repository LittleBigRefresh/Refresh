using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class ReviewEndpoints : EndpointGroup
{
    [GameEndpoint("dpadrate/{type}/{levelId}", HttpMethods.Post)]
    public Response SubmitRating(RequestContext context, GameDatabaseContext database, GameUser user, int levelId)
    {
        GameLevel? level = database.GetLevelById(levelId);
        if (level == null) return NotFound;

        bool parsed = sbyte.TryParse(context.QueryString.Get("rating"), out sbyte rating);
        if (!parsed) return BadRequest;

        bool rated = database.RateLevel(level, user, (RatingType)rating);
        return rated ? OK : Unauthorized;
    }
    
    [GameEndpoint("rate/user/{id}", ContentType.Xml, HttpMethods.Post)]
    [AllowEmptyBody]
    public Response RateUserLevel(RequestContext context, GameDatabaseContext database, GameUser user, int id)
    {
        string? ratingString = context.QueryString.Get("rating");
        
        if (ratingString == null) return BadRequest;

        if (!int.TryParse(ratingString, out int ratingInt)) return BadRequest;
        
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

        GameLevel? level = database.GetLevelById(id);

        if (level == null)
        {
            return NotFound;
        }
            
        return database.RateLevel(level, user, rating) ? OK : Unauthorized;

    } 
}