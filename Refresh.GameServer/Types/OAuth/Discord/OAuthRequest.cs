using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.OAuth.Discord;

public partial class OAuthRequest : IRealmObject
{
    [PrimaryKey]
    public string State { get; set; }
    public GameUser User { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    
    [Ignored]
    public OAuthProvider Provider
    {
        get => (OAuthProvider)this._Provider;
        set => this._Provider = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    public int _Provider { get; set; }
}