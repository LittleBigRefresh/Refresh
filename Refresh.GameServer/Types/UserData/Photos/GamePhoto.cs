using Realms;
using Refresh.GameServer.Database;

namespace Refresh.GameServer.Types.UserData.Photos;

#nullable disable

public partial class GamePhoto : IRealmObject, ISequentialId
{
    [PrimaryKey] public int PhotoId { get; set; }
    public DateTimeOffset TakenAt { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    
    public GameUser Publisher { get; set; }
    
    public string SmallHash { get; set; }
    public string MediumHash { get; set; }
    public string LargeHash { get; set; }
    public string PlanHash { get; set; }
    
    public IList<GamePhotoSubject> Subjects { get; }
    
    public int SequentialId
    {
        set => this.PhotoId = value;
    }
}