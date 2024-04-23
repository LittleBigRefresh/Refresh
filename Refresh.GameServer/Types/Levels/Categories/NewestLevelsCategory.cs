using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class NewestLevelsCategory : LevelCategory
{
    internal NewestLevelsCategory() : base("newest", "newest", false)
    {
        this.Name = "Newest Levels";
        this.Description = "Levels that were most recently uploaded!";
        this.IconHash = "g820623";
        this.FontAwesomeIcon = "calendar";
    }
    
    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, MatchService matchService, GameDatabaseContext database, GameUser? accessor,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => database.GetNewestLevels(count, skip, accessor, levelFilterSettings);
}