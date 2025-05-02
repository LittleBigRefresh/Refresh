using System.Diagnostics;
using Refresh.Database.Models.Activity;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Relations;
namespace Refresh.Database;

public partial class GameDatabaseContext // ActivityRead
{
    public GameUser? GetUserFromEvent(Event e)
    {
        if (e.StoredDataType != EventDataType.User)
            throw new InvalidOperationException($"Event does not store the correct data type (expected {nameof(EventDataType.User)})");

        Debug.Assert(e.StoredObjectId != null);

        return this.GetUserByObjectId(e.StoredObjectId);
    }

    public GameLevel? GetLevelFromEvent(Event e)
    {
        if (e.StoredDataType != EventDataType.Level)
            throw new InvalidOperationException($"Event does not store the correct data type (expected {nameof(EventDataType.Level)})");

        Debug.Assert(e.StoredSequentialId != null);
        
        return this.GetLevelById(e.StoredSequentialId.Value);
    }

    public GameSubmittedScore? GetScoreFromEvent(Event e)
    {
        if (e.StoredDataType != EventDataType.Score)
            throw new InvalidOperationException($"Event does not store the correct data type (expected {nameof(EventDataType.Score)})");

        Debug.Assert(e.StoredObjectId != null);

        return this.GetScoreByObjectId(e.StoredObjectId);
    }

    public GamePhoto? GetPhotoFromEvent(Event e)
    {
        if (e.StoredDataType != EventDataType.Photo)
            throw new InvalidOperationException($"Event does not store the correct data type (expected {nameof(EventDataType.Photo)})");

        Debug.Assert(e.StoredSequentialId != null);

        return this.GetPhotoById(e.StoredSequentialId.Value);
    }
    
    public RateLevelRelation? GetRateLevelRelationFromEvent(Event e)
    {
        if (e.StoredDataType != EventDataType.RateLevelRelation)
            throw new InvalidOperationException($"Event does not store the correct data type (expected {nameof(EventDataType.RateLevelRelation)})");

        Debug.Assert(e.StoredObjectId != null);

        return this.RateLevelRelations
            .FirstOrDefault(l => l.RateLevelRelationId == e.StoredObjectId);
    }
}