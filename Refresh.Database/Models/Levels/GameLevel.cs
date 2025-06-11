using System.Xml.Serialization;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;

#if POSTGRES
using PrimaryKeyAttribute = Refresh.Database.Compatibility.PrimaryKeyAttribute;
#endif

namespace Refresh.Database.Models.Levels;

[JsonObject(MemberSerialization.OptIn)]
[Index(nameof(Title), nameof(Description), nameof(StoryId))]
public partial class GameLevel : IRealmObject, ISequentialId
{
    [Key, PrimaryKey] public int LevelId { get; set; }
    
    public bool IsAdventure { get; set; }

    [Indexed(IndexType.FullText)]
    public string Title { get; set; } = "";
    public string IconHash { get; set; } = "0";
    [Indexed(IndexType.FullText)]
    public string Description { get; set; } = "";

    public int LocationX { get; set; }
    public int LocationY { get; set; }

    public string RootResource { get; set; } = string.Empty;

    /// <summary>
    /// When the level was first published in unix milliseconds
    /// </summary>
    public DateTimeOffset PublishDate { get; set; }
    /// <summary>
    /// When the level was last updated in unix milliseconds
    /// </summary>
    public DateTimeOffset UpdateDate { get; set; }
    
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public bool EnforceMinMaxPlayers { get; set; }
    
    public bool SameScreenGame { get; set; }
    [Ignored, NotMapped]
    public bool TeamPicked => this.DateTeamPicked != null;
    public DateTimeOffset? DateTeamPicked { get; set; }
    
    /// <summary>
    /// Whether any asset in the dependency tree is considered "modded"
    /// </summary>
    public bool IsModded { get; set; }
    
    /// <summary>
    /// The GUID of the background, this seems to only be used by LBP PSP
    /// </summary>
    public string? BackgroundGuid { get; set; }
    
    public TokenGame GameVersion 
    {
        get => (TokenGame)this._GameVersion;
        set => this._GameVersion = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    internal int _GameVersion { get; set; }
    
    public GameLevelType LevelType
    {
        get => (GameLevelType)this._LevelType;
        set => this._LevelType = (int)value;
    }

    // ReSharper disable once InconsistentNaming
    internal int _LevelType { get; set; }

    /// <summary>
    /// The associated ID for the developer level.
    /// Set to 0 for user generated levels, since slot IDs of zero are invalid ingame.
    /// </summary>
    [Indexed] public int StoryId { get; set; }

    public GameSlotType SlotType 
        => this.StoryId == 0 ? GameSlotType.User : GameSlotType.Story;
    
    public bool IsLocked { get; set; }
    public bool IsSubLevel { get; set; }
    public bool IsCopyable { get; set; }
    public bool RequiresMoveController { get; set; }
    
    /// <summary>
    /// The score, used for Cool Levels.
    /// </summary>
    /// <seealso cref="CoolLevelsWorker"/>
    public float Score { get; set; }

#nullable disable
    // ILists can't be serialized to XML, and Lists/Arrays cannot be stored in realm,
    // hence _SkillRewards and SkillRewards both existing
    // ReSharper disable once InconsistentNaming
    public IList<GameSkillReward> _SkillRewards { get; }
    
    public IList<GameReview> Reviews { get; }
    
#nullable restore
    
    [XmlArray("customRewards")]
    [XmlArrayItem("customReward")]
    public GameSkillReward[] SkillRewards
    {
        get => this._SkillRewards.ToArray();
        set
        {
            this._SkillRewards.Clear();
            value = value.OrderBy(r=>r.Id).ToArray();
            
            // There should never be more than 3 skill rewards
            for (int i = 0; i < Math.Min(value.Length, 3); i++)
            {
                GameSkillReward reward = value[i];
                reward.Id = i;
                this._SkillRewards.Add(reward);
            }
        }
    }
    
    public int SequentialId
    {
        get => this.LevelId;
        set => this.LevelId = value;
    }

    public GameUser? Publisher { get; set; }
    /// <summary>
    /// The publisher who originally published the level, if it has been re-uploaded by someone else.
    /// Should only be set if the original publisher does not have an account.
    /// </summary>
    public string? OriginalPublisher { get; set; }

    public bool IsReUpload { get; set; }

    /// <summary>
    /// Calculates the average rating of a level based on the ratings it has.
    /// </summary>
    /// <returns>A double between 1 and 5, indicating the level's average ratings.</returns>
    public double CalculateAverageStarRating(GameDatabaseContext database)
    {
        int yayCount = database.GetTotalRatingsForLevel(this, RatingType.Yay);
        int booCount = database.GetTotalRatingsForLevel(this, RatingType.Boo);
        int neutralCount = database.GetTotalRatingsForLevel(this, RatingType.Neutral);

        // Return 0 if all the counts are 0, we don't want a div by 0 error!
        if (yayCount + booCount + neutralCount == 0) return 0;
        
        return (double)((5 * yayCount) + (1 * booCount) + (3 * neutralCount)) / (yayCount + booCount + neutralCount);
    }
    
    /// <summary>
    /// Provides a unique level ID for ~1.1 billion hashed levels, uses the hash directly, so this is deterministic
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    public static int LevelIdFromHash(string hash)
    {
        const int rangeStart = 1_000_000_000;
        const int rangeEnd = int.MaxValue;
        const int range = rangeEnd - rangeStart;
        
        return rangeStart + Math.Abs(hash.GetHashCode()) % range;
    }
}