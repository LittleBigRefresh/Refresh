using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class MostUniquelyPlayedLevelsCategory : LevelCategory
{
    internal MostUniquelyPlayedLevelsCategory() : base("mostPlayed", "mostUniquePlays", false)
    {
        this.Name = "Starter Pack";
        this.Description = "Levels that many people have played.";
        this.FontAwesomeIcon = "play";
        this.IconHash = "g820608";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        MatchService matchService, IGameDatabaseContext database, GameUser? user,
        LevelFilterSettings levelFilterSettings) 
        => database.GetMostUniquelyPlayedLevels(count, skip, user, levelFilterSettings);
}