using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Relations;
#nullable disable

public class FavouriteUserRelation : RealmObject
{
    public GameUser UserToFavourite { get; set; }
    public GameUser UserFavouriting { get; set; }
}