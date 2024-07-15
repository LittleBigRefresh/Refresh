using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Reviews;

namespace Refresh.GameServer.Types.Comments;

[XmlRoot("comment")]
[XmlType("comment")]
public class SerializedComment : IDataConvertableFrom<SerializedComment, GameProfileComment>, IDataConvertableFrom<SerializedComment, GameLevelComment>
{
    [XmlElement("id")] public required int CommentId { get; set; }
    
    [XmlElement("message")] public required string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp in Unix milliseconds
    /// </summary>
    [XmlElement("timestamp")] public required long Timestamp { get; set; } 
    
    // Comments are special; they do not include icons in the npHandle
    [XmlElement("npHandle")] public required string? Handle { get; set; }
    
    [XmlElement("thumbsup")] public int? ThumbsUp { get; set; }
    [XmlElement("thumbsdown")] public int? ThumbsDown { get; set; }
    [XmlElement("yourthumb")] public int? YourThumb { get; set; }

    private static SerializedComment FromBase(IGameComment comment) =>
        new()
        {
            CommentId = comment.SequentialId,
            Content = comment.Content,
            Timestamp = comment.Timestamp,
            Handle = comment.Author.Username,
        };

    public static SerializedComment? FromOld(GameProfileComment? old, DataContext dataContext)
    {
        if (old == null) return null;

        SerializedComment comment = FromBase(old);
        comment.ThumbsUp = dataContext.Database.GetTotalRatingsForProfileComment(old, RatingType.Yay);
        comment.ThumbsDown = dataContext.Database.GetTotalRatingsForProfileComment(old, RatingType.Boo);
        comment.YourThumb = (int?)dataContext.Database.GetProfileCommentRatingByUser(old, dataContext.User!) ?? 0;
        return comment;
    }
    
    public static SerializedComment? FromOld(GameLevelComment? old, DataContext dataContext)
    {
        if (old == null) return null;

        SerializedComment comment = FromBase(old);
        comment.ThumbsUp = dataContext.Database.GetTotalRatingsForLevelComment(old, RatingType.Yay);
        comment.ThumbsDown = dataContext.Database.GetTotalRatingsForLevelComment(old, RatingType.Boo);
        comment.YourThumb = (int?)dataContext.Database.GetLevelCommentRatingByUser(old, dataContext.User!) ?? 0;
        return comment;
    }

    public static IEnumerable<SerializedComment> FromOldList(IEnumerable<GameProfileComment> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
    public static IEnumerable<SerializedComment> FromOldList(IEnumerable<GameLevelComment> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}