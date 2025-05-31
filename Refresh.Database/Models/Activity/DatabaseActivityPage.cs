using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Activity;

public class DatabaseActivityPage
{
    internal DatabaseActivityPage(GameDatabaseContext database, IReadOnlyCollection<Event> events)
    {
        this.StoreReferencedObjects(database, events);
        this.GenerateGroups(events);
    }
    
    public DateTimeOffset Start;
    public DateTimeOffset End;
    
    public List<DatabaseActivityGroup> EventGroups = [];

    public List<GameUser> Users = [];
    public List<GameLevel> Levels = [];
    public List<GameSubmittedScore> Scores = [];
    public List<GamePhoto> Photos = [];

    private void StoreReferencedObjects(GameDatabaseContext database, IReadOnlyCollection<Event> events)
    {
        // Users
        this.Users.AddRange(events
            .Select(e => e.User)
            .DistinctBy(e => e.UserId));

        this.Users.AddRange(events.Where(e => e.StoredDataType == EventDataType.User)
            .DistinctBy(e => e.StoredObjectId)
            .Select(e => database.GetUserFromEvent(e)!));

        // Levels
        this.Levels.AddRange(events
            .Where(e => e.StoredDataType == EventDataType.Level)
            .DistinctBy(e => e.StoredSequentialId)
            .Select(e => database.GetLevelFromEvent(e)!));
        
        // Photos
        this.Photos.AddRange(events
            .Where(e => e.StoredDataType == EventDataType.Photo)
            .DistinctBy(e => e.StoredSequentialId)
            .Select(e => database.GetPhotoFromEvent(e)!));
    }

    private void GenerateGroups(IReadOnlyCollection<Event> events)
    {
        foreach (EventDataType type in Enum.GetValues<EventDataType>())
        {
            switch (type)
            {
                case EventDataType.User:
                    this.GenerateUserGroups(events);
                    break;
                case EventDataType.Level:
                    this.GenerateLevelGroups(events);
                    break;
                case EventDataType.Score:
                    this.GenerateScoreGroups(events);
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
    }

    private void GenerateUserGroups(IReadOnlyCollection<Event> events)
    {
        foreach (Event @event in events.Where(e => e.StoredDataType == EventDataType.User))
        {
            GameUser user = this.Users.First(u => u.UserId == @event.StoredObjectId);

            this.EventGroups.Add(new DatabaseActivityUserGroup(user)
            {
                Timestamp = @event.Timestamp,
                Children = [
                    new DatabaseActivityUserGroup(@event.User)
                    {
                        Timestamp = @event.Timestamp,
                        Events = [@event],
                    },
                ],
            });
        }
    }
    
    private void GenerateLevelGroups(IReadOnlyCollection<Event> events)
    {
        foreach (Event @event in events.Where(e => e.StoredDataType is EventDataType.Level or EventDataType.Photo))
        {
            int levelId = @event.StoredSequentialId!.Value;

            if (@event.StoredDataType == EventDataType.Photo)
            {
                GamePhoto photo = this.Photos.First(p => p.PhotoId == @event.StoredSequentialId);
                levelId = photo.LevelId;
            }

            GameLevel level = this.Levels.First(l => l.LevelId == levelId);
            
            this.EventGroups.Add(new DatabaseActivityLevelGroup(level)
            {
                Timestamp = @event.Timestamp,
                Children = [
                    new DatabaseActivityUserGroup(@event.User)
                    {
                        Timestamp = @event.Timestamp,
                        Events = [@event],
                    },
                ],
            });
        }
    }
    
    private void GenerateScoreGroups(IReadOnlyCollection<Event> events)
    {
        foreach (Event @event in events.Where(e => e.EventType == EventType.LevelScore))
        {
            GameSubmittedScore score = this.Scores.First(u => u.ScoreId == @event.StoredObjectId);
            GameLevel level = score.Level;

            this.EventGroups.Add(new DatabaseActivityLevelGroup(level)
            {
                Timestamp = @event.Timestamp,
                Children = [
                    new DatabaseActivityUserGroup(@event.User)
                    {
                        Timestamp = @event.Timestamp,
                        Events = [@event],
                    },
                ],
            });
        }
    }
}