using System.Xml.Serialization;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Photos;

[XmlRoot("subject")]
[XmlType("subject")]
public class GamePhotoSubject
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    [Obsolete("used for serialization. XML stuff should be moved to SerializedGamePhotoSubject", true)]
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