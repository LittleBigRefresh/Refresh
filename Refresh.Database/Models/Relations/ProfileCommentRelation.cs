using MongoDB.Bson;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

public partial class ProfileCommentRelation : IRealmObject, ICommentRelation<GameProfileComment>
{
    [Key] public ObjectId CommentRelationId { get; set; } = ObjectId.GenerateNewId();
    #if POSTGRES
    [Required]
    #endif
    public GameUser User { get; set; }
    #if POSTGRES
    [Required]
    #endif
    public GameProfileComment Comment { get; set; }
    [Ignored, NotMapped]
    public RatingType RatingType
    {
        get => (RatingType)this._RatingType;
        set => this._RatingType = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    public int _RatingType { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}