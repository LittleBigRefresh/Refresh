using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Core.Types.Matching;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

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
        ResultFilterSettings levelFilterSettings, GameUser? _)
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