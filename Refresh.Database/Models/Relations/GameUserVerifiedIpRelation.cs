using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

public partial class GameUserVerifiedIpRelation : IRealmObject
{
    public GameUser User { get; set; }
    public string IpAddress { get; set; }
    public DateTimeOffset VerifiedAt { get; set; }
}