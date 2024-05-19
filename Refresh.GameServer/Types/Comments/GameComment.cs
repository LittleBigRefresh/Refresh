using System.Xml.Serialization;
using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Refresh.GameServer.Types.Comments;

[XmlRoot("comment")]
[XmlType("comment")]
public partial class GameComment : IRealmObject, ISequentialId
{
    [PrimaryKey] [XmlElement("id")] public int SequentialId { get; set; }

    [XmlIgnore] public GameUser Author { get; set; } = null!;
    [XmlElement("message")] public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp in Unix milliseconds
    /// </summary>
    [XmlElement("timestamp")] public long Timestamp { get; set; } 

    #region LBP Serialization Quirks
    
    // Comments are special; they do not include icons in the npHandle
    [XmlElement("npHandle")] [Ignored] public string? Handle { get; set; }
    [XmlElement("thumbsup")] [Ignored] public int? ThumbsUp { get; set; }
    [XmlElement("thumbsdown")] [Ignored] public int? ThumbsDown { get; set; }
    [XmlElement("yourthumb")] [Ignored] public int? YourThumb { get; set; }

    public void PrepareForSerialization(GameUser user, DataContext dataContext)
    {
        this.Handle = this.Author.Username;
        
        this.ThumbsUp = dataContext.Database.GetTotalRatingsForComment(this, RatingType.Yay);
        this.ThumbsDown = dataContext.Database.GetTotalRatingsForComment(this, RatingType.Boo);
        
        this.YourThumb = (int?)dataContext.Database.GetRatingByUser(this, user) ?? 0;
    }
    
    #endregion

}
