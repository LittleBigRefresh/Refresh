using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Relations;
#nullable disable

public partial class FavouriteUserRelation : IRealmObject
{
    public GameUser UserToFavourite { get; set; }
    public GameUser UserFavouriting { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}