using Realms;

namespace Refresh.GameServer.Types.UserData.Photos;

public partial class GamePhotoSubject : IEmbeddedObject
{
    public GameUser? User { get; set; }

    public string DisplayName { get; set; }
    public IList<float> Bounds { get; }
}