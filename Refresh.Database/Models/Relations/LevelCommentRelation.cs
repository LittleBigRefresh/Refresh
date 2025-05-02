using MongoDB.Bson;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Comments.Relations;

public partial class LevelCommentRelation : IRealmObject, ICommentRelation<GameLevelComment>
{
    public ObjectId CommentRelationId { get; set; } = ObjectId.GenerateNewId();
    public GameUser User { get; set; }
    public GameLevelComment Comment { get; set; }
    [Ignored]
    public RatingType RatingType
    {
        get => (RatingType)this._RatingType;
        set => this._RatingType = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    internal int _RatingType { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}