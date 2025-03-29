using System.Xml.Serialization;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Types.Activity.Groups;
using Refresh.GameServer.Types.Activity.SerializedEvents;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Photos;
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
    public IEnumerable<Event> Events { get; set; }
    
    [XmlIgnore]
    public List<GameUser> Users { get; set; } = null!;

    [XmlIgnore]
    public List<GameLevel> Levels { get; set; } = null!;

    [JsonIgnore]
    [XmlElement("groups")]
    public ActivityGroups Groups { get; set; }

    [XmlElement("users")]
    public SerializedUserList SerializedUsers { get; set; }
    
    [XmlElement("slots")]
    public SerializedLevelList SerializedLevels { get; set; }
    
    [XmlIgnore, JsonIgnore]
    public List<GameSubmittedScore> Scores { get; set; }
    
    [XmlIgnore, JsonIgnore]
    public List<GamePhoto> Photos { get; set; }

    public ActivityPage()
    {
        this.Events = new List<Event>();
        this.Groups = new ActivityGroups();
        this.SerializedLevels = new SerializedLevelList();
        this.SerializedUsers = new SerializedUserList();
        this.Scores = new List<GameSubmittedScore>();
        this.Photos = new List<GamePhoto>();
    }

    private void FillInInfo(GameDatabaseContext database, bool generateGroups, ActivityQueryParameters parameters,
        DataContext dataContext)
    {
        List<GameUser> users = this.Events
            .Select(e => e.User)
            .DistinctBy(e => e.UserId)
            .ToList();

        users.AddRange(this.Events.Where(e => e.StoredDataType == EventDataType.User)
            .DistinctBy(e => e.StoredObjectId)
            .Select(e => database.GetUserFromEvent(e)!));

        this.SerializedUsers = new SerializedUserList
        {
            Items = GameUserResponse.FromOldList(users, dataContext).ToList(),
        };

        this.Users = users;

        List<GameLevel> levels = this.Events
            .Where(e => e.StoredDataType == EventDataType.Level)
            .DistinctBy(e => e.StoredSequentialId)
            .Select(e => database.GetLevelFromEvent(e)!) // probably pretty inefficient
            .ToList();

        this.SerializedLevels = new SerializedLevelList
        {
            Items = GameLevelResponse.FromOldList(levels, dataContext).ToList(),
        };

        this.Levels = levels;
        
        List<GameSubmittedScore> scores = this.Events
            .Where(e => e.StoredDataType == EventDataType.Score)
            .DistinctBy(e => e.StoredObjectId)
            .Select(e => database.GetScoreByObjectId(e.StoredObjectId))
            .ToList()!;

        this.Scores = scores;
        
        List<GamePhoto> photos = this.Events
            .Where(e => e.StoredDataType == EventDataType.Photo)
            .DistinctBy(e => e.StoredSequentialId)
            .Select(e => database.GetPhotoById(e.StoredSequentialId!.Value))
            .ToList()!;

        this.Photos = photos;

        this.Groups = generateGroups ? this.GenerateGroups(users, scores, database) : new ActivityGroups();

        this.Groups.Groups = this.Groups.Groups
            .OrderByDescending(g => g.Timestamp)
            .ToList();

        if (this.Events.Any())
        {
            this.StartTimestamp = parameters.Timestamp;
            this.EndTimestamp = parameters.EndTimestamp;
        } 
    }

    public static ActivityPage GameForLevelActivity(GameDatabaseContext database,
        GameLevel level,
        ActivityQueryParameters parameters, DataContext dataContext)
    {
        DatabaseList<Event> events = database.GetRecentActivityForLevel(level, parameters);

        ActivityPage page = new()
        {
            Events = events.Items,
        };
        
        page.FillInInfo(database, true, parameters, dataContext);
        
        page.Groups.Groups = page.Groups.Groups.SelectMany(group => group.Subgroups?.Items ?? []).ToList();
        
        return page;
    }
    
    public static ActivityPage ApiForLevelActivity(GameDatabaseContext database,
        GameLevel level,
        ActivityQueryParameters parameters,
        DataContext dataContext,
        bool generateGroups = true)
    {
        DatabaseList<Event> events = database.GetRecentActivityForLevel(level, parameters);

        ActivityPage page = new()
        {
            Events = events.Items,
        };
        
        page.FillInInfo(database, generateGroups, parameters, dataContext);
        
        return page;
    }
    
    public static ActivityPage ApiFromUserActivity(GameDatabaseContext database,
        ActivityQueryParameters parameters,
        DataContext dataContext,
        bool generateGroups = true)
    {
        DatabaseList<Event> events = database.GetRecentActivityFromUser(parameters);

        ActivityPage page = new()
        {
            Events = events.Items,
        };
        
        page.FillInInfo(database, generateGroups, parameters, dataContext);
        
        return page;
    }
    
    public static ActivityPage GameFromUserActivity(GameDatabaseContext database,
        ActivityQueryParameters parameters, DataContext dataContext)
    {
        DatabaseList<Event> events = database.GetRecentActivityFromUser(parameters);

        ActivityPage page = new()
        {
            Events = events.Items,
        };
        
        page.FillInInfo(database, true, parameters, dataContext);
        
        page.Groups.Groups = page.Groups.Groups.SelectMany(group => group.Subgroups?.Items ?? []).ToList();
        
        return page;
    }
    
    public static ActivityPage GameUserActivity(GameDatabaseContext database,
        ActivityQueryParameters parameters,
        DataContext dataContext,
        bool generateGroups = true)
    {
        DatabaseList<Event> events = database.GetUserRecentActivity(parameters);

        ActivityPage page = new()
        {
            Events = events.Items,
        };
        
        page.FillInInfo(database, generateGroups, parameters, dataContext);
        
        return page;
    }
    
    public static ActivityPage GlobalActivity(GameDatabaseContext database,
        ActivityQueryParameters parameters,
        DataContext dataContext,
        bool generateGroups = true)
    {
        DatabaseList<Event> events = database.GetGlobalRecentActivity(parameters);

        ActivityPage page = new()
        {
            Events = events.Items,
        };
        
        page.FillInInfo(database, generateGroups, parameters, dataContext);
        
        return page;
    }
    
    private ActivityGroups GenerateGroups(IReadOnlyCollection<GameUser> users, IReadOnlyCollection<GameSubmittedScore> scores, GameDatabaseContext database)
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
                    this.GenerateLevelGroups(groups, database);
                    break;
                case EventDataType.Score:
                    this.GenerateScoreGroups(groups, scores);
                    break;
                case EventDataType.RateLevelRelation:
                    // TODO
                    break;
                case EventDataType.Photo:
                    // This case is handled by the `Level` part, since the game expects photos to appear in the level groups
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return groups;
    }

    private void GenerateLevelGroups(ActivityGroups groups, GameDatabaseContext database)
    {
        foreach (Event @event in this.Events.Where(e => e.StoredDataType is EventDataType.Level or EventDataType.Photo))
        {
            GamePhoto? photo = null;
            int levelId = @event.StoredSequentialId!.Value;

            if (@event.StoredDataType == EventDataType.Photo)
            {
                photo = database.GetPhotoFromEvent(@event)!;
                levelId = photo.LevelId;
            }
            
            SerializedLevelId id = new()
            {
                LevelId = levelId,
                Type = "user",
            };

            long timestamp = @event.Timestamp.ToUnixTimeMilliseconds();

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
                EventType.LevelUpload => SerializedLevelUploadEvent.FromSerializedLevelEvent(levelEvent),
                EventType.LevelPlay => SerializedLevelPlayEvent.FromSerializedLevelEvent(levelEvent),
                EventType.PhotoUpload => SerializedPhotoUploadEvent.FromSerializedLevelEvent(levelEvent, photo!),
                _ => levelEvent,
            };

            groups.Groups.Add(new LevelActivityGroup
            {
                LevelId = id,
                Timestamp = timestamp,
                Subgroups = new Subgroups([
                    new UserActivityGroup
                    {
                        Username = @event.User.Username,
                        Timestamp = timestamp,
                        Events = new Events([
                            levelEvent,
                        ]),
                    },

                ]),
            });
        }
    }
    
    private void GenerateScoreGroups(ActivityGroups groups, IReadOnlyCollection<GameSubmittedScore> scores)
    {
        foreach (Event @event in this.Events.Where(e => e.EventType == EventType.LevelScore))
        {
            GameSubmittedScore score = scores.First(u => u.ScoreId == @event.StoredObjectId);
            
            SerializedLevelId id = new()
            {
                LevelId = score.Level.LevelId,
                Type = "user",
            };

            long timestamp = @event.Timestamp.ToUnixTimeMilliseconds();

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
                Subgroups = new Subgroups([
                    new UserActivityGroup
                    {
                        Username = @event.User.Username,
                        Timestamp = timestamp,
                        Events = new Events([
                            scoreEvent,
                        ]),
                    },

                ]),
            });
        }
    }
    
    private void GenerateUserGroups(ActivityGroups groups, IReadOnlyCollection<GameUser> users)
    {
        foreach (Event @event in this.Events.Where(e => e.StoredDataType == EventDataType.User))
        {
            long timestamp = @event.Timestamp.ToUnixTimeMilliseconds();

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
                Subgroups = new Subgroups([
                    new UserActivityGroup
                    {
                        Username = @event.User.Username,
                        Timestamp = timestamp,
                        Events = new Events([
                            userEvent,
                        ]),
                    },

                ]),
            });
        }
    }
}