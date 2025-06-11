using MongoDB.Bson;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

public partial class ProfileCommentRelation : IRealmObject, ICommentRelation<GameProfileComment>
{
    public ObjectId CommentRelationId { get; set; } = ObjectId.GenerateNewId();
    public GameUser User { get; set; }
    public GameProfileComment Comment { get; set; }
    [Ignored, NotMapped]
    public RatingType RatingType
    {
        get => (RatingType)this._RatingType;
        set => this._RatingType = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    internal int _RatingType { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}