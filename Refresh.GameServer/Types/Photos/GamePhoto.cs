using Newtonsoft.Json;
using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Photos;

#nullable disable

[JsonObject(MemberSerialization.OptOut)]
public partial class GamePhoto : IRealmObject, ISequentialId
{
    [PrimaryKey] public int PhotoId { get; set; }
    public DateTimeOffset TakenAt { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    
    public GameUser Publisher { get; set; }
    #nullable restore
    public GameLevel? Level { get; set; }
    #nullable disable
    
    public string LevelName { get; set; }
    public string LevelType { get; set; }
    public int LevelId { get; set; }
    
    public string SmallHash { get; set; }
    public string MediumHash { get; set; }
    public string LargeHash { get; set; }
    public string PlanHash { get; set; }
    
    public IList<GamePhotoSubject> Subjects { get; }
    
    [JsonIgnore] public int SequentialId
    {
        set => this.PhotoId = value;
    }
}