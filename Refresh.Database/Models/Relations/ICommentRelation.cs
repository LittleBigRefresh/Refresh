using MongoDB.Bson;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

public interface ICommentRelation<TComment> : IRealmObject
    where TComment : IGameComment
{
    ObjectId CommentRelationId { get; set; }
    GameUser User { get; set; }
    TComment Comment { get; set; }
    RatingType RatingType { get; set; }
    DateTimeOffset Timestamp { get; set; }
}