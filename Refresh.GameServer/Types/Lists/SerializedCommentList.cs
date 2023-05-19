using System.Xml.Serialization;
using Refresh.GameServer.Types.Comments;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("comments")]
[XmlType("comments")]
public class SerializedCommentList
{
    public SerializedCommentList() {}
    
    public SerializedCommentList(IEnumerable<GameComment> comments)
    {
        this.Items = comments.ToList();
    }

    [XmlElement("comment")]
    public List<GameComment> Items { get; set; } = new();
}