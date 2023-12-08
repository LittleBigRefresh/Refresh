using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;
using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Photos;

[XmlRoot("subject")]
[XmlType("subject")]
[Keyless] // TODO: fix
public partial class GamePhotoSubject : IEmbeddedObject
{
    public GameUser? User { get; set; }

    public string DisplayName { get; set; }
    public IList<float> Bounds { get; }
}