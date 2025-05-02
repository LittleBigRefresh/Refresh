using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Challenges.LbpHub;

public partial class GameChallenge : IRealmObject, ISequentialId
{
    [PrimaryKey] public int ChallengeId { get; set; }
    
    public string Name { get; set; } = "";
    public GameUser? Publisher { get; set; }

    #nullable disable
    public GameLevel Level { get; set; }
    #nullable enable

    /// <summary>
    /// The Uid of the checkpoint this challenge starts on.
    /// </summary>
    public int StartCheckpointUid { get; set; }
    /// <summary>
    /// The Uid of the checkpoint this challenge finishes on.
    /// </summary>
    public int FinishCheckpointUid { get; set; }
    /// <summary>
    /// The challenge's criteria type (time/score/lives etc).
    /// </summary>
    /// <seealso cref="GameChallengeCriteriaType"/>
    public GameChallengeCriteriaType Type
    {
        get => (GameChallengeCriteriaType)this._Type;
        set => this._Type = (byte)value;
    }
    public byte _Type { get; set; }

    public DateTimeOffset PublishDate { get; set; }
    public DateTimeOffset LastUpdateDate { get; set; }
    public DateTimeOffset ExpirationDate { get; set; }

    public int SequentialId
    {
        get => this.ChallengeId;
        set => this.ChallengeId = value;
    }
}