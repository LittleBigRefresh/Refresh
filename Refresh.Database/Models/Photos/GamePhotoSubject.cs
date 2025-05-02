using System.Xml.Serialization;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Photos;

[XmlRoot("subject")]
[XmlType("subject")]
public class GamePhotoSubject
{
    public GamePhotoSubject() {}

    public GamePhotoSubject(GameUser? user, string displayName, IList<float> bounds)
    {
        this.User = user;
        this.DisplayName = displayName;
        this.Bounds = bounds;
    }

    public GameUser? User { get; set; }
    
    public string DisplayName { get; set; }
    public IList<float> Bounds { get; }
}