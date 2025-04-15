namespace Refresh.GameServer.Types.Pins;

public class GamePin
{
    /// <summary>
    /// The identifier used by the game to group together pins with the same objective and different target values
    /// in-game. Also the only value the game sends to identify pins, so we have to use it.
    /// </summary>
    public long ProgressType { get; set; }
    public byte Category { get; set; }
    public string TranslatedName { get; set; } = "";
    public string TranslatedDescription { get; set; } = "";
    public int[] TargetValues { get; set; } = [];
}