using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class HighestRatedLevelsCategory : LevelCategory
{
    internal HighestRatedLevelsCategory() : base("mostLiked", new[] { "thumbs", "highestRated" }, false)
    {
        this.Name = "Highest Rated";
        this.Description = "Levels with the most Yays!";
        this.FontAwesomeIcon = "thumbs-up";
        this.IconHash = "g820603";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        MatchService matchService, GameDatabaseContext database, GameUser? accessor,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => database.GetHighestRatedLevels(count, skip, accessor, levelFilterSettings);
}