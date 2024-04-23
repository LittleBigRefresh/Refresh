using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Relations;
#nullable disable

public partial class CommentRelation : IRealmObject
{
    public ObjectId CommentRelationId { get; set; } = ObjectId.GenerateNewId();
    public GameUser User { get; set; }
    public GameComment Comment { get; set; }
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