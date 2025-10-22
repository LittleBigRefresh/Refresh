using System.Diagnostics;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Database.Models.Activity;

public class DatabaseActivityPage
{
    internal DatabaseActivityPage(GameDatabaseContext database, IReadOnlyCollection<Event> events, ActivityQueryParameters parameters)
    {
        this.StoreReferencedObjects(database, events);
        this.GenerateGroups(events);
        
        this.Cleanup();
        
        this.Start = DateTimeOffset.MaxValue;
        this.End = DateTimeOffset.MinValue;
        foreach (DatabaseActivityGroup group in this.EventGroups)
        {
            group.TraverseChildrenForEventsRecursively((e) =>
            {
                if (e.Timestamp > this.End)
                    this.End = e.Timestamp;

                if (e.Timestamp < this.Start)
                    this.Start = e.Timestamp;
            });
        }

        // go back 1 week by default to look for next page
        if(this.End != DateTimeOffset.MinValue)
            this.End = this.End.Subtract(TimeSpan.FromDays(7));
    }
    
    public DateTimeOffset Start;
    public DateTimeOffset End;
    
    public readonly List<DatabaseActivityGroup> EventGroups = [];
    private List<DatabaseActivityUserGroup>? UserEventGroups = [];
    private List<DatabaseActivityLevelGroup>? LevelEventGroups = [];

    public readonly List<GameUser> Users = [];
    public readonly List<GameLevel> Levels = [];
    public readonly List<GameScore> Scores = [];
    public readonly List<GamePhoto> Photos = [];

    #region Generation

    private void StoreReferencedObjects(GameDatabaseContext database, IReadOnlyCollection<Event> events)
    {
        // Users
        // Add actors
        List<GameUser> users = events
            .Select(e => e.User)
            .DistinctBy(e => e.UserId)
            .ToList();

        this.Users.AddRange(events.Where(e => e.StoredDataType == EventDataType.User)
            .DistinctBy(e => e.StoredObjectId)
            .Select(e => database.GetUserFromEvent(e)!));
        
        // Add all involved users which are not null, and all undeleted object users
        foreach (Event e in events.Where(e => e.StoredDataType == EventDataType.User))
        {
            if (e.InvolvedUser != null) users.Add(e.InvolvedUser);

            GameUser? obj = database.GetUserFromEvent(e);
            if (obj != null) users.Add(obj);
        }

        // Distinguish users since we've gotten them from 3 different sources, and then add
        // the resulting list to the class list attribute
        this.Users.AddRange(users.DistinctBy(u => u.UserId));

        // Photos
        List<GamePhoto> photos = [];
        foreach (Event e in events.Where(e => e.StoredDataType == EventDataType.Photo))
        {
            GamePhoto? obj = database.GetPhotoFromEvent(e);
            if (obj != null) photos.Add(obj);
        }
        this.Photos.AddRange(photos.DistinctBy(p => p.PhotoId));
        
        // Scores
        List<GameScore> scores = [];
        foreach (Event e in events.Where(e => e.StoredDataType == EventDataType.Score))
        {
            GameScore? obj = database.GetScoreFromEvent(e);
            if (obj != null) scores.Add(obj);
        }
        this.Scores.AddRange(scores.DistinctBy(s => s.ScoreId));
        
        // Levels
        List<GameLevel> levels = [];
        foreach (Event e in events.Where(e => e.StoredDataType == EventDataType.Score))
        {
            GameLevel? obj = database.GetLevelFromEvent(e);
            if (obj != null) levels.Add(obj);
        }
        this.Levels.AddRange(levels.DistinctBy(l => l.LevelId));
        
        // Levels (from photos)
        foreach (GamePhoto photo in this.Photos)
        {
            if(photo.Level == null)
                continue;

            if(this.Levels.Contains(photo.Level))
               continue;
            
            this.Levels.Add(photo.Level);
        }
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
                case EventDataType.Review:
                case EventDataType.UserComment:
                case EventDataType.LevelComment:
                case EventDataType.Playlist:
                case EventDataType.Challenge:
                case EventDataType.ChallengeScore:
                case EventDataType.Contest:
                case EventDataType.Asset:
                    // Ignore for now. TODO: Implement these aswell
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
            
            DatabaseActivityUserGroup group = GetOrCreateUserUserGroup(@event, user, @event.User);
            group.Events.Add(@event);
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
                levelId = photo.LevelId ?? 0;
            }

            GameLevel? level = this.Levels.FirstOrDefault(l => l.LevelId == levelId);
            if (level == null)
            {
                DatabaseActivityUserGroup userGroup = GetOrCreateUserGroup(@event);
                userGroup.Events.Add(@event);
                
                continue;
            }
            
            DatabaseActivityUserGroup group = GetOrCreateLevelUserGroup(@event, level);
            group.Events.Add(@event);
        }
    }

    private void GenerateScoreGroups(IReadOnlyCollection<Event> events)
    {
        foreach (Event @event in events.Where(e => e.EventType == EventType.LevelScore))
        {
            GameScore score = this.Scores.First(u => u.ScoreId == @event.StoredObjectId);
            GameLevel level = score.Level;

            DatabaseActivityUserGroup group = GetOrCreateLevelUserGroup(@event, level);
            group.Events.Add(@event);
        }
    }

    private void Cleanup()
    {
        foreach (DatabaseActivityGroup group in this.EventGroups)
        {
            group.Cleanup();
        }
        
        this.UserEventGroups = null;
        this.LevelEventGroups = null;
    }

    #endregion

    #region Group Creation

    private DatabaseActivityUserGroup GetOrCreateUserUserGroup(Event @event, GameUser user1, GameUser user2)
    {
        Debug.Assert(this.UserEventGroups != null);
        
        DatabaseActivityUserGroup? rootGroup = this.UserEventGroups.FirstOrDefault(u => u.User.UserId == user1.UserId);
        if (rootGroup == null)
        {
            rootGroup = new DatabaseActivityUserGroup(user1)
            {
                Timestamp = @event.Timestamp,
            };
            this.EventGroups.Add(rootGroup);
            this.UserEventGroups.Add(rootGroup);
        }

        Debug.Assert(rootGroup.UserChildren != null);
        
        DatabaseActivityUserGroup? subGroup = rootGroup.UserChildren.FirstOrDefault(u => u.User.UserId == user2.UserId);
        if (subGroup == null)
        {
            subGroup = new DatabaseActivityUserGroup(user2)
            {
                Timestamp = @event.Timestamp,
            };
            rootGroup.Children.Add(subGroup);
            rootGroup.UserChildren.Add(subGroup);
        }

        return subGroup;
    }
    
    private DatabaseActivityUserGroup GetOrCreateUserGroup(Event @event, GameUser user)
    {
        Debug.Assert(this.UserEventGroups != null);
        
        DatabaseActivityUserGroup? rootGroup = this.UserEventGroups.FirstOrDefault(u => u.User.UserId == user.UserId);
        if (rootGroup == null)
        {
            rootGroup = new DatabaseActivityUserGroup(user)
            {
                Timestamp = @event.Timestamp,
            };
            this.EventGroups.Add(rootGroup);
            this.UserEventGroups.Add(rootGroup);
        }

        return rootGroup;
    }
    
    private DatabaseActivityUserGroup GetOrCreateUserGroup(Event @event)
        => this.GetOrCreateUserGroup(@event, @event.User);

    private DatabaseActivityUserGroup GetOrCreateLevelUserGroup(Event @event,  GameLevel level, GameUser user)
    {
        Debug.Assert(this.LevelEventGroups != null);
        
        DatabaseActivityLevelGroup? rootGroup = this.LevelEventGroups.FirstOrDefault(u => u.Level.LevelId == level.LevelId);
        if (rootGroup == null)
        {
            rootGroup = new DatabaseActivityLevelGroup(level)
            {
                Timestamp = @event.Timestamp,
            };
            this.EventGroups.Add(rootGroup);
            this.LevelEventGroups.Add(rootGroup);
        }

        Debug.Assert(rootGroup.UserChildren != null);
        
        DatabaseActivityUserGroup? subGroup = rootGroup.UserChildren.FirstOrDefault(u => u.User.UserId == user.UserId);
        if (subGroup == null)
        {
            subGroup = new DatabaseActivityUserGroup(user)
            {
                Timestamp = @event.Timestamp,
            };
            rootGroup.Children.Add(subGroup);
            rootGroup.UserChildren.Add(subGroup);
        }

        return subGroup;
    }

    private DatabaseActivityUserGroup GetOrCreateLevelUserGroup(Event @event, GameLevel level)
        => GetOrCreateLevelUserGroup(@event, level, @event.User);

    #endregion
}