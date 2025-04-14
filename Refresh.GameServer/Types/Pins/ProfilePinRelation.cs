using Refresh.GameServer.Authentication;

namespace Refresh.GameServer.Types.Pins;

#nullable disable
public partial class ProfilePinRelation : IRealmObject
{
    public PinProgressRelation Pin { get; set; }
    /// <summary>
    /// Whether to show as first, second or third pin in-game
    /// </summary>
    public byte Index { get; set; }
    public TokenGame GameVersion 
    {
        get => (TokenGame)this._GameVersion;
        set => this._GameVersion = (int)value;
    }
    
    internal int _GameVersion { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}