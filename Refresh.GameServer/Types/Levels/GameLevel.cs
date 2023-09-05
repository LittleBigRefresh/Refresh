using System.Xml.Serialization;
using Realms;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.Levels.SkillRewards;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Types.Levels;

[JsonObject(MemberSerialization.OptIn)]
public partial class GameLevel : IRealmObject, ISequentialId
{
    [PrimaryKey] public int LevelId { get; set; }

    [Indexed(IndexType.FullText)]
    public string Title { get; set; } = "";
    public string IconHash { get; set; } = "0";
    [Indexed(IndexType.FullText)]
    public string Description { get; set; } = "";
    public GameLocation Location { get; set; } = GameLocation.Zero;

    public string RootResource { get; set; } = string.Empty;

    public long PublishDate { get; set; } // unix seconds
    public long UpdateDate { get; set; }
    
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public bool EnforceMinMaxPlayers { get; set; }
    
    public bool SameScreenGame { get; set; }
    public bool TeamPicked { get; set; }
    
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
    
    public bool IsLocked { get; set; }
    public bool IsSubLevel { get; set; }
    public bool IsCopyable { get; set; }

#nullable disable
    public IList<GameComment> LevelComments { get; }
    
    [Backlink(nameof(FavouriteLevelRelation.Level))]
    public IQueryable<FavouriteLevelRelation> FavouriteRelations { get; }
    
    [Backlink(nameof(UniquePlayLevelRelation.Level))]
    public IQueryable<UniquePlayLevelRelation> UniquePlays { get; }
    
    [Backlink(nameof(PlayLevelRelation.Level))]
    public IQueryable<PlayLevelRelation> AllPlays { get; }
    [Backlink(nameof(GameSubmittedScore.Level))]
    public IQueryable<GameSubmittedScore> Scores { get; }
    
    [Backlink(nameof(RateLevelRelation.Level))]
    public IQueryable<RateLevelRelation> Ratings { get; }
    
    // ILists can't be serialized to XML, and Lists/Arrays cannot be stored in realm,
    // hence _SkillRewards and SkillRewards both existing
    // ReSharper disable once InconsistentNaming
    public IList<GameSkillReward> _SkillRewards { get; }
    
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
        set => this.LevelId = value;
    }

    public GameUser? Publisher { get; set; }
}