using System.Diagnostics;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

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