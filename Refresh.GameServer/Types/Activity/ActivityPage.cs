using System.Xml.Serialization;
using Newtonsoft.Json;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Activity.Groups;
using Refresh.GameServer.Types.Activity.SerializedEvents;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Activity;

[Serializable]
[XmlRoot("stream")]
[XmlType("stream")]
public class ActivityPage
{
    [XmlElement("start_timestamp")]
    public long StartTimestamp { get; set; }
    
    [XmlElement("end_timestamp")]
    public long EndTimestamp { get; set; }
    
    [XmlIgnore]
    public List<Event> Events { get; set; }

    [JsonIgnore]
    [XmlElement("groups")]
    public ActivityGroups Groups { get; set; }

    [XmlElement("users")]
    public SerializedUserList Users { get; set; }
    
    [XmlElement("slots")]
    public SerializedLevelList Levels { get; set; }

    public ActivityPage()
    {
        this.Events = new List<Event>();
        this.Groups = new ActivityGroups();
        this.Levels = new SerializedLevelList();
        this.Users = new SerializedUserList();
    }

    public ActivityPage(GameDatabaseContext database, int count = 20, int skip = 0, long timestamp = 0, long endTimestamp = 0, bool generateGroups = true)
    {
        if (timestamp == 0) timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        this.Events = new List<Event>(database.GetRecentActivity(count, skip, timestamp, endTimestamp));
        
        List<GameUser> users = this.Events
            .Select(e => e.User)
            .DistinctBy(e => e.UserId)
            .ToList();

        users.AddRange(this.Events.Where(e => e.StoredDataType == EventDataType.User)
            .DistinctBy(e => e.StoredObjectId)
            .Select(e => database.GetUserFromEvent(e)!));

        this.Users = new SerializedUserList
        {
            Users = users,
        };

        List<GameLevel> levels = this.Events
            .Where(e => e.StoredDataType == EventDataType.Level)
            .DistinctBy(e => e.StoredSequentialId)
            .Select(e => database.GetLevelFromEvent(e)!) // probably pretty inefficient
            .ToList();

        this.Levels = new SerializedLevelList
        {
            Items = levels,
        };

        this.Groups = generateGroups ? this.GenerateGroups(users) : new ActivityGroups();

        if (this.Events.Count > 0)
        {
            this.StartTimestamp = timestamp;
            this.EndTimestamp = endTimestamp;
        }
    }

    private ActivityGroups GenerateGroups(IReadOnlyCollection<GameUser> users)
    {
        ActivityGroups groups = new();
        
        foreach (EventDataType type in Enum.GetValues<EventDataType>())
        {
            switch (type)
            {
                case EventDataType.User:
                    this.GenerateUserGroups(groups, users);
                    break;
                case EventDataType.Level:
                    this.GenerateLevelGroups(groups);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return groups;
    }

    private void GenerateLevelGroups(ActivityGroups groups)
    {
        foreach (Event @event in this.Events.Where(e => e.StoredDataType == EventDataType.Level))
        {
            SerializedLevelId id = new()
            {
                LevelId = @event.StoredSequentialId!.Value,
                Type = "user",
            };

            long timestamp = @event.Timestamp;

            SerializedLevelEvent levelEvent = new()
            {
                Type = @event.EventType,
                Timestamp = timestamp,
                LevelId = id,
                Actor = @event.User.Username,
            };

            // Level upload events have special properties
            // cant put extra properties as nullable in base, nullables will still be serialized
            if (@event.EventType == EventType.LevelUpload)
                levelEvent = SerializedLevelUploadEvent.FromSerializedLevelEvent(levelEvent);

            groups.Groups.Add(new LevelActivityGroup
            {
                LevelId = id,
                Timestamp = timestamp,
                Subgroups = new Subgroups(new List<ActivityGroup>
                {
                    new UserActivityGroup
                    {
                        Username = @event.User.Username,
                        Timestamp = timestamp,
                        Events = new Events(new List<SerializedEvent>
                        {
                            levelEvent,
                        }),
                    },
                }),
            });
        }
    }
    
    private void GenerateUserGroups(ActivityGroups groups, IReadOnlyCollection<GameUser> users)
    {
        foreach (Event @event in this.Events.Where(e => e.StoredDataType == EventDataType.User))
        {
            long timestamp = @event.Timestamp;

            GameUser user = users.First(u => u.UserId == @event.StoredObjectId);

            SerializedUserEvent userEvent = new()
            {
                Type = @event.EventType,
                Timestamp = timestamp,
                Actor = @event.User.Username,
                Username = user.Username,
            };

            groups.Groups.Add(new UserActivityGroup
            {
                Username = user.Username,
                Timestamp = timestamp,
                Subgroups = new Subgroups(new List<ActivityGroup>
                {
                    new UserActivityGroup
                    {
                        Username = @event.User.Username,
                        Timestamp = timestamp,
                        Events = new Events(new List<SerializedEvent>
                        {
                            userEvent,
                        }),
                    },
                }),
            });
        }
    }
}