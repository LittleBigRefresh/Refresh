using System.Xml.Serialization;
using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

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

    public void PrepareForSerialization()
    {
        this.Handle = this.Author.Username;
        
        this.ThumbsUp = 0;
        this.ThumbsDown = 0;
        
        this.YourThumb = 0;
    }
    
    #endregion

}
