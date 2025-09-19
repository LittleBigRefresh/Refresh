using System.Diagnostics;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Statistics;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Levels;

[JsonObject(MemberSerialization.OptIn)]
[Index(nameof(Title), nameof(Description), nameof(StoryId))]
public partial class GameLevel : ISequentialId
{
    [Key] public int LevelId { get; set; }
    
    public bool IsAdventure { get; set; }

    public string Title { get; set; } = "";
    public string IconHash { get; set; } = "0";
    public string Description { get; set; } = "";

    public int LocationX { get; set; }
    public int LocationY { get; set; }

    public List<Label> Labels { get; set; } = [];

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
    [NotMapped]
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
    
    public TokenGame GameVersion  { get; set; }
    public GameLevelType LevelType { get; set; }

    /// <summary>
    /// The associated ID for the developer level.
    /// Set to 0 for user generated levels, since slot IDs of zero are invalid ingame.
    /// </summary>
    public int StoryId { get; set; }

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
    public float CoolRating { get; set; }
    
    [NotMapped] public int SequentialId
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

    public GameLevelStatistics? Statistics { get; set; }

    public bool IsReUpload { get; set; }

    /// <summary>
    /// Calculates the average rating of a level based on the ratings it has.
    /// </summary>
    /// <returns>A double between 1 and 5, indicating the level's average ratings.</returns>
    public double CalculateAverageStarRating()
    {
        Debug.Assert(this.Statistics != null);
        int yayCount = this.Statistics.YayCount;
        int booCount = this.Statistics.BooCount;
        int neutralCount = this.Statistics.NeutralCount;
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

    public GameLevel Clone()
    {
        return (GameLevel)this.MemberwiseClone();
    }
}