using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Assets;
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
    
    public GameAsset SmallAsset { get; set; }
    public GameAsset MediumAsset { get; set; }
    public GameAsset LargeAsset { get; set; }
    public string PlanHash { get; set; }
    
    [Ignored]
    public IReadOnlyCollection<GamePhotoSubject> Subjects
    {
        get
        {
            List<GamePhotoSubject> subjects = new(4);
            
            if (this.Subject1DisplayName != null)
                subjects.Add(new GamePhotoSubject(this.Subject1User, this.Subject1DisplayName, this.Subject1Bounds));
            else return subjects;
            
            if (this.Subject2DisplayName != null)
                subjects.Add(new GamePhotoSubject(this.Subject2User, this.Subject2DisplayName, this.Subject2Bounds));
            else return subjects;
            
            if (this.Subject3DisplayName != null)
                subjects.Add(new GamePhotoSubject(this.Subject3User, this.Subject3DisplayName, this.Subject3Bounds));
            else return subjects;
            
            if (this.Subject4DisplayName != null)
                subjects.Add(new GamePhotoSubject(this.Subject4User, this.Subject4DisplayName, this.Subject4Bounds));

            return subjects;
        }
    }

#nullable enable
    #pragma warning disable CS8618 // realm forces us to have a non-nullable IList<float> so we have to have these shenanigans
    
    public GameUser? Subject1User { get; set; }
    public string? Subject1DisplayName { get; set; }
    public IList<float> Subject1Bounds { get; }
    
    public GameUser? Subject2User { get; set; }
    public string? Subject2DisplayName { get; set; }
    public IList<float> Subject2Bounds { get; }
    
    public GameUser? Subject3User { get; set; }
    public string? Subject3DisplayName { get; set; }
    public IList<float> Subject3Bounds { get; }
    
    public GameUser? Subject4User { get; set; }
    public string? Subject4DisplayName { get; set; }
    public IList<float> Subject4Bounds { get; }
    
    #pragma warning restore CS8618
    #nullable disable
    
    [JsonIgnore] public int SequentialId
    {
        set => this.PhotoId = value;
    }
}