using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Comments.Relations;

public interface ICommentRelation<TComment> : IRealmObject
    where TComment : IGameComment
{
    ObjectId CommentRelationId { get; set; }
    GameUser User { get; set; }
    TComment Comment { get; set; }
    RatingType RatingType { get; set; }
    DateTimeOffset Timestamp { get; set; }
}