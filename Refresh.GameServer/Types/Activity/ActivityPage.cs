using System.Xml.Serialization;
using Newtonsoft.Json;
using Refresh.GameServer.Database;
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
    public List<Event> Events { get; set; } = new();

    [JsonIgnore]
    [XmlElement("groups")]
    public ActivityGroups Groups { get; set; } = new();

    [XmlElement("users")]
    public GameUserList Users { get; set; }
    
    [XmlElement("slots")]
    public GameLevelList Levels { get; set; }

    public ActivityPage()
    {
        this.Levels = new GameLevelList();
        this.Users = new GameUserList();
    }

    public ActivityPage(RealmDatabaseContext database, int count = 20, int skip = 0)
    {
        this.Events = new List<Event>(database.GetRecentActivity(count, skip));

        // TODO: verify that users and levels cannot have duplicates
        List<GameUser> users = this.Events
            .Select(e => e.User)
            .ToList();

        this.Users = new GameUserList
        {
            Users = users,
        };

        List<GameLevel> levels = this.Events
            .Where(e => e.StoredDataType == EventDataType.Level)
            .Select(e => database.GetGameLevelFromEvent(e)!) // probably pretty inefficient
            .ToList();

        this.Levels = new GameLevelList
        {
            Items = levels,
        };

        this.Groups = new ActivityGroups
        {
            Groups = this.Events,
        };
    }
}