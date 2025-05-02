using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Pins;

#nullable disable
public partial class ProfilePinRelation : IRealmObject
{
    public long PinId { get; set; }
    public GameUser Publisher { get; set; }

    /// <summary>
    /// Whether to show as first, second or third pin in-game
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// The game to show this profile pin on, to allow different sets of profile pins per game
    /// </summary>
    public TokenGame Game
    {
        get => (TokenGame)this._Game;
        set => this._Game = (int)value;
    }
    internal int _Game { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}