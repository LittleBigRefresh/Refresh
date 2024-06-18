using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Reviews;

namespace Refresh.GameServer.Types.Comments;

[XmlRoot("comment")]
[XmlType("comment")]
public class SerializedComment : IDataConvertableFrom<SerializedComment, GameComment>
{
    [XmlElement("id")] public required int CommentId { get; set; }
    
    [XmlElement("message")] public required string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp in Unix milliseconds
    /// </summary>
    [XmlElement("timestamp")] public required long Timestamp { get; set; } 
    
    // Comments are special; they do not include icons in the npHandle
    [XmlElement("npHandle")] public required string? Handle { get; set; }
    
    [XmlElement("thumbsup")] public required int? ThumbsUp { get; set; }
    [XmlElement("thumbsdown")] public required int? ThumbsDown { get; set; }
    [XmlElement("yourthumb")] public required int? YourThumb { get; set; }
    
    public static SerializedComment? FromOld(GameComment? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new SerializedComment
        {
            CommentId = old.SequentialId,
            Content = old.Content,
            Timestamp = old.Timestamp,
            Handle = old.Author.Username,
            ThumbsUp = dataContext.Database.GetTotalRatingsForComment(old, RatingType.Yay),
            ThumbsDown = dataContext.Database.GetTotalRatingsForComment(old, RatingType.Boo),
            YourThumb = (int?)dataContext.Database.GetRatingByUser(old, dataContext.User!) ?? 0,
        };
    }

    public static IEnumerable<SerializedComment> FromOldList(IEnumerable<GameComment> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}