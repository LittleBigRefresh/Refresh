using Refresh.GameServer.Authentication;

namespace Refresh.GameServer.Types.Pins;

#nullable disable
public partial class ProfilePinRelation : IRealmObject
{
    /// <summary>
    /// Contains both the pin to show, and the user to show under, ensuring the user
    /// has at least some progress on this pin as a low bar
    /// </summary>
    public PinProgressRelation Pin { get; set; }

    /// <summary>
    /// Whether to show as first, second or third pin in-game
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// The game to show this profile pin on, to allow different sets of profile pins
    /// per game
    /// </summary>
    public TokenGame Game
    {
        get => (TokenGame)this._Game;
        set => this._Game = (int)value;
    }
    internal int _Game { get; set; }

    /// <summary>
    /// When this relation was created, aka when <see cref='Pin'/> and <see cref='Index'/> were last updated
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}