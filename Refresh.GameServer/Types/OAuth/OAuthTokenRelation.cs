using JetBrains.Annotations;
using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.OAuth;

#nullable disable

public partial class OAuthTokenRelation : IRealmObject
{
    public GameUser User { get; set; }
    
    [Ignored]
    public OAuthProvider Provider
    {
        get => (OAuthProvider)this._Provider;
        set => this._Provider = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    public int _Provider { get; set; }
    
    /// <summary>
    /// The user's access token
    /// </summary>
    public string AccessToken { get; set; }
    /// <summary>
    /// The time the access token gets revoked
    /// </summary>
    public DateTimeOffset AccessTokenRevocationTime { get; set; }
    /// <summary>
    /// The refresh token used to get a new access token
    /// </summary>
    [CanBeNull] public string RefreshToken { get; set; }
}