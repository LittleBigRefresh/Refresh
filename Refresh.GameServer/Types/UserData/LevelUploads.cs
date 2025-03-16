using Realms;

namespace Refresh.GameServer.Types.UserData;

public partial class LevelUploads : IRealmObject
{
    public int Count { get; set; }

    public bool DateIsExpired { get; set; }

    public DateTimeOffset ExpiryDate { get; set; } = DateTimeOffset.Now;
}