using System.Xml.Serialization;
using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;
using Bunkum.HttpServer.Serialization;
using Refresh.GameServer.Types.Levels.CustomRewards;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Types.Levels;

[XmlRoot("slot")]
[XmlType("slot")]
[JsonObject(MemberSerialization.OptIn)]
public partial class GameLevel : IRealmObject, INeedsPreparationBeforeSerialization, ISequentialId
{
    [PrimaryKey] [XmlElement("id")] [JsonProperty] public int LevelId { get; set; }

    [XmlElement("name")] [JsonProperty] public string Title { get; set; } = "";
    [XmlElement("icon")] [JsonProperty] public string IconHash { get; set; } = "0";
    [XmlElement("description")] [JsonProperty] public string Description { get; set; } = "";
    [XmlElement("location")] [JsonProperty] public GameLocation Location { get; set; } = GameLocation.Zero;

    [XmlElement("rootLevel")] public string RootResource { get; set; } = string.Empty;

    [XmlElement("firstPublished")] [JsonProperty] public long PublishDate { get; set; } // unix seconds
    [XmlElement("lastUpdated")] [JsonProperty] public long UpdateDate { get; set; }

#nullable disable
    [Backlink(nameof(FavouriteLevelRelation.Level))]
    [XmlIgnore] public IQueryable<FavouriteLevelRelation> FavouriteRelations { get; }
    
    [Backlink(nameof(UniquePlayLevelRelation.Level))]
    [XmlIgnore] public IQueryable<UniquePlayLevelRelation> UniquePlays { get; }
    
    [Backlink(nameof(PlayLevelRelation.Level))]
    [XmlIgnore] public IQueryable<PlayLevelRelation> AllPlays { get; }
    [Backlink(nameof(GameSubmittedScore.Level))]
    [XmlIgnore] public IQueryable<GameSubmittedScore> Scores { get; }
    
    // ILists can't be serialized to XML, and Lists/Arrays cannot be stored in realm,
    // hence _CustomRewards and CustomRewards both existing
    // ReSharper disable once InconsistentNaming
    public IList<CustomReward> _CustomRewards { get; }
    
#nullable restore
    
    [JsonProperty]
    [XmlArray("customRewards")]
    [XmlArrayItem("customReward")]
    public CustomReward[] CustomRewards
    {
        get => this._CustomRewards.ToArray();
        set
        {
            this._CustomRewards.Clear();
            value = value.OrderBy(r=>r.Id).ToArray();
            
            // There should never be more than 3 skill rewards
            for (int i = 0; i < Math.Min(value.Length, 3); i++)
            {
                CustomReward reward = value[i];
                reward.Id = i;
                this._CustomRewards.Add(reward);
            }
        }
    }
    
    public int SequentialId
    {
        set => this.LevelId = value;
    }

    [XmlIgnore] [JsonProperty] public GameUser? Publisher { get; set; }
    
    #region LBP Serialization Quirks
    [Ignored] [XmlAttribute("type")] public string? Type { get; set; }

    [Ignored] [XmlElement("npHandle")] public SerializedUserHandle? Handle { get; set; }
    
    [Ignored] [XmlElement("heartCount")] public int? HeartCount { get; set; }
    
    [Ignored] [XmlElement("playCount")] public int? TotalPlayCount { get; set; }
    [Ignored] [XmlElement("uniquePlayCount")] public int? UniquePlayCount { get; set; }

    [Ignored] [XmlElement("resource")] public List<string> XmlResources { get; set; } = new();

    public void PrepareForSerialization()
    {
        this.HeartCount = this.FavouriteRelations.Count();
        this.TotalPlayCount = this.AllPlays.Count();
        this.UniquePlayCount = this.UniquePlays.Count();

        if (this.Publisher != null)
        {
            this.Type = "user";
            this.Handle = SerializedUserHandle.FromUser(this.Publisher);
        }
        else
        {
            this.Type = "developer";
        }
    }
    #endregion
}