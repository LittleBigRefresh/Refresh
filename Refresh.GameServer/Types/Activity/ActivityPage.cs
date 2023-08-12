using System.Xml.Serialization;
using Newtonsoft.Json;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Types.Activity.Groups;
using Refresh.GameServer.Types.Activity.SerializedEvents;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

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
    
    [XmlIgnore]
    public List<GameUser> Users { get; set; }
    
    [XmlIgnore]
    public List<GameLevel> Levels { get; set; }

    [JsonIgnore]
    [XmlElement("groups")]
    public ActivityGroups Groups { get; set; }

    [XmlElement("users")]
    public SerializedUserList SerializedUsers { get; set; }
    
    [XmlElement("slots")]
    public SerializedLevelList SerializedLevels { get; set; }
    
    [XmlIgnore, JsonIgnore]
    public List<GameSubmittedScore> Scores { get; set; }

    public ActivityPage()
    {
        this.Events = new List<Event>();
        this.Groups = new ActivityGroups();
        this.SerializedLevels = new SerializedLevelList();
        this.SerializedUsers = new SerializedUserList();
        this.Scores = new List<GameSubmittedScore>();
    }

    public ActivityPage(GameDatabaseContext database, int count = 20, int skip = 0, long timestamp = 0, long endTimestamp = 0, bool generateGroups = true, GameLevel? level = null)
    {
        if (timestamp == 0) timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        DatabaseList<Event> events;
        
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (level != null)
            events = database.GetRecentActivityForLevel(level, count, skip, timestamp, endTimestamp);
        else
            events = database.GetRecentActivity(count, skip, timestamp, endTimestamp);

        this.Events = new List<Event>(events.Items);
        
        List<GameUser> users = this.Events
            .Select(e => e.User)
            .DistinctBy(e => e.UserId)
            .ToList();

        users.AddRange(this.Events.Where(e => e.StoredDataType == EventDataType.User)
            .DistinctBy(e => e.StoredObjectId)
            .Select(e => database.GetUserFromEvent(e)!));

        this.SerializedUsers = new SerializedUserList
        {
            Users = GameUserResponse.FromOldList(users).ToList(),
        };

        this.Users = users;

        List<GameLevel> levels = this.Events
            .Where(e => e.StoredDataType == EventDataType.Level)
            .DistinctBy(e => e.StoredSequentialId)
            .Select(e => database.GetLevelFromEvent(e)!) // probably pretty inefficient
            .ToList();

        this.SerializedLevels = new SerializedLevelList
        {
            Items = GameLevelResponse.FromOldList(levels).ToList(),
        };

        this.Levels = levels;
        
        List<GameSubmittedScore> scores = this.Events
            .Where(e => e.StoredDataType == EventDataType.Score)
            .DistinctBy(e => e.StoredObjectId)
            .Select(e => database.GetScoreByObjectId(e.StoredObjectId))
            .ToList()!;

        this.Scores = scores;

        this.Groups = generateGroups ? this.GenerateGroups(users, scores) : new ActivityGroups();

        this.Groups.Groups = this.Groups.Groups
            .OrderByDescending(g => g.Timestamp)
            .ToList();

        if (this.Events.Count > 0)
        {
            this.StartTimestamp = timestamp;
            this.EndTimestamp = endTimestamp;
        }
    }

    private ActivityGroups GenerateGroups(IReadOnlyCollection<GameUser> users, IReadOnlyCollection<GameSubmittedScore> scores)
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
                case EventDataType.Score:
                    this.GenerateScoreGroups(groups, scores);
                    break;
                case EventDataType.RateLevelRelation:
                    // TODO
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

            // Events can sometimes have special properties
            // We can't put extra properties as nullable in the base class, as nullables will still be serialized
            //
            // AND HEY! Remember to add an [XmlInclude] in SerializedEvent when adding something here!
            // You will waste 30 seconds of your time if you don't.
            levelEvent = @event.EventType switch
            {
                EventType.Level_Upload => SerializedLevelUploadEvent.FromSerializedLevelEvent(levelEvent),
                EventType.Level_Play => SerializedLevelPlayEvent.FromSerializedLevelEvent(levelEvent),
                _ => levelEvent,
            };

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
    
    private void GenerateScoreGroups(ActivityGroups groups, IReadOnlyCollection<GameSubmittedScore> scores)
    {
        foreach (Event @event in this.Events.Where(e => e.EventType == EventType.SubmittedScore_Create))
        {
            GameSubmittedScore score = scores.First(u => u.ScoreId == @event.StoredObjectId);
            
            SerializedLevelId id = new()
            {
                LevelId = score.Level.LevelId,
                Type = "user",
            };

            long timestamp = @event.Timestamp;

            SerializedScoreSubmitEvent scoreEvent = new()
            {
                Type = @event.EventType,
                Timestamp = timestamp,
                LevelId = id,
                Actor = @event.User.Username,
                Score = score.Score,
                ScoreType = score.ScoreType,
            };

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
                            scoreEvent,
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