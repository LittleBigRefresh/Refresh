using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Activity;

[Serializable]
public class ActivityPage
{
    public List<Event> Events { get; set; } = new();

    public List<GameUser> Users { get; set; } = new();
    public List<GameLevel> Levels { get; set; } = new();

    public ActivityPage() {}

    public ActivityPage(IEnumerable<Event> events)
    {
        this.Events = new List<Event>(events);

        this.Users = new List<GameUser>();
        this.Levels = new List<GameLevel>();

        IEnumerable<GameUser> usersToAdd = this.Events
            .Select(e => e.User)
            .Where(u => !this.Users.Contains(u));
        
        this.Users.AddRange(usersToAdd);
    }
}