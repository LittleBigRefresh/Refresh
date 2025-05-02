using Bunkum.Core;
using Refresh.Database.Extensions;
using Refresh.Database.Query;
using Refresh.Database;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Levels;

public class CurrentlyPlayingCategory : GameLevelCategory
{
    internal CurrentlyPlayingCategory() : base("currentlyPlaying", "busiest", false)
    {
        this.Name = "Popular Now";
        this.Description = "Levels people are playing right now!";
        this.IconHash = "g820602";
        this.FontAwesomeIcon = "users";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _)
    {
        IOrderedEnumerable<IGrouping<GameLevel?,GameRoom>> rooms = dataContext.Match.RoomAccessor.GetAllRooms()
            .Where(r => r.LevelType == RoomSlotType.Online && r.HostId.Id != null) // if playing online level and host exists on server
            .GroupBy(r => dataContext.Database.GetLevelById(r.LevelId))
            .OrderBy(r => r.Sum(room => room.PlayerIds.Count));

        return new DatabaseList<GameLevel>(rooms.Select(r => r.Key)
            .Where(l => l != null && l.StoryId == 0)!
            .FilterByLevelFilterSettings(dataContext.User, levelFilterSettings)
            .FilterByGameVersion(levelFilterSettings.GameVersion), skip, count);
    }
}