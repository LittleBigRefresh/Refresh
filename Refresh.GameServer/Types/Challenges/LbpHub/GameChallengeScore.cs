using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Challenges.LbpHub;

#nullable disable

public partial class GameChallengeScore : IRealmObject, ISequentialId
{
    [PrimaryKey] public int ScoreId { get; set; }

    public GameChallenge Challenge { get; set; }
    public GameUser Publisher { get; set; }
    /// <summary>
    /// The publisher's achieved raw score. More always means better here, independent of challenge type.
    /// </summary>
    public long Score { get; set; }
    /// <summary>
    /// The hash of the ghost asset for this score.
    /// </summary>
    public string GhostHash { get; set; } = "";
    /// <summary>
    /// How long it took the publisher to achieve this score. Calculated by subtracting the first checkpoint's activation time 
    /// from the last checkpoint's activation time in the score's ghost asset.
    /// </summary>
    public long Time { get; set; }
    public DateTimeOffset PublishDate { get; set; }

    public int SequentialId
    {
        get => this.ScoreId;
        set => this.ScoreId = value;
    }
}