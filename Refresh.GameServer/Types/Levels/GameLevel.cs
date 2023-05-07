using System.Xml.Serialization;
using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;
using Bunkum.HttpServer.Serialization;
using Newtonsoft.Json;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Types.Levels;

[XmlRoot("slot")]
[XmlType("slot")]
[JsonObject(MemberSerialization.OptIn)]
public partial class GameLevel : IRealmObject, INeedsPreparationBeforeSerialization, ISequentialId
{
    [PrimaryKey] [Indexed] [XmlElement("id")] [JsonProperty] public int LevelId { get; set; }
    
    [XmlElement("name")] [JsonProperty] public string Title { get; set; }
    [XmlElement("icon")] [JsonProperty] public string IconHash { get; set; }
    [XmlElement("description")] [JsonProperty] public string Description { get; set; }
    [XmlElement("location")] [JsonProperty] public GameLocation Location { get; set; } = GameLocation.Zero;

    [XmlElement("rootLevel")] public string RootResource { get; set; } = string.Empty;
    [XmlIgnore] public IList<string> Resources { get; } = new List<string>();
    
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
    #nullable restore
    
    public int SequentialId
    {
        set => this.LevelId = value;
    }

    [XmlIgnore] [JsonProperty] public GameUser? Publisher { get; set; }
    
    #region LBP Serialization Quirks
    [Ignored] [XmlAttribute("type")] public string? Type { get; set; }

    [Ignored] [XmlElement("npHandle")] public NameAndIcon? Handle { get; set; }
    
    [Ignored] [XmlElement("heartCount")] public int? HeartCount { get; set; }
    
    [Ignored] [XmlElement("playCount")] public int? TotalPlayCount { get; set; }
    [Ignored] [XmlElement("uniquePlayCount")] public int? UniquePlayCount { get; set; }

    // Realm cant have IList types with setters? Fine, I'll play your game. ;p
    [Ignored]
    [XmlElement("resource")]
    public string[] XmlResources
    {
        get => this.Resources.ToArray();
        set
        {
            this.Resources.Clear();
            
            foreach (string r in value) 
                this.Resources.Add(r);
        }
    }

    public void PrepareForSerialization()
    {
        this.HeartCount = this.FavouriteRelations.Count();
        this.TotalPlayCount = this.AllPlays.Count();
        this.UniquePlayCount = this.UniquePlays.Count();

        if (this.Publisher != null)
        {
            this.Type = "user";
            this.Handle = NameAndIcon.FromUser(this.Publisher);
        }
        else
        {
            this.Type = "developer";
        }
    }
    #endregion
}