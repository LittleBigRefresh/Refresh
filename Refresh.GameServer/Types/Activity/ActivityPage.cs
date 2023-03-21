using System.Xml.Serialization;
using Newtonsoft.Json;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Activity.Groups;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Activity;

[Serializable]
[XmlRoot("stream")]
[XmlType("stream")]
public class ActivityPage
{
    [XmlIgnore]
    public List<Event> Events { get; set; }

    [JsonIgnore]
    [XmlElement("groups")]
    public ActivityGroups Groups { get; set; }

    [XmlElement("users")]
    public GameUserList Users { get; set; }
    
    [XmlElement("slots")]
    public GameLevelList Levels { get; set; }

    public ActivityPage()
    {
        this.Events = new List<Event>();
        this.Groups = new ActivityGroups();
        this.Levels = new GameLevelList();
        this.Users = new GameUserList();
    }

    public ActivityPage(RealmDatabaseContext database, int count = 20, int skip = 0, bool generateGroups = true)
    {
        this.Events = new List<Event>(database.GetRecentActivity(count, skip));
        
        List<GameUser> users = this.Events
            .Select(e => e.User)
            .DistinctBy(e => e.UserId)
            .ToList();

        this.Users = new GameUserList
        {
            Users = users,
        };

        List<GameLevel> levels = this.Events
            .Where(e => e.StoredDataType == EventDataType.Level)
            .Select(e => database.GetLevelFromEvent(e)!) // probably pretty inefficient
            .DistinctBy(e => e.LevelId)
            .ToList();

        this.Levels = new GameLevelList
        {
            Items = levels,
        };

        this.Groups = generateGroups ? this.GenerateGroups(levels, users) : new ActivityGroups();
    }

    private ActivityGroups GenerateGroups(List<GameLevel> levels, List<GameUser> users)
    {
        ActivityGroups groups = new();
        
        foreach (EventDataType type in Enum.GetValues<EventDataType>())
        {
            foreach (Event @event in this.Events.Where(e => e.StoredDataType == type))
            {
                groups.Groups.Add(new LevelActivityGroup
                {
                    LevelId = @event.StoredSequentialId!.Value,
                    Timestamp = @event.Timestamp,
                    Subgroups = new Subgroups(new List<ActivityGroup>
                    {
                        new UserActivityGroup
                        {
                            Username = @event.User.Username,
                            Timestamp = @event.Timestamp,
                            Events = new Events(new List<Event>
                            {
                                @event,
                            }),
                        },
                    }),
                });
            }
        }

        return groups;
    }
}