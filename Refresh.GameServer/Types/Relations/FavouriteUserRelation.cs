using Microsoft.EntityFrameworkCore;
using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Relations;
#nullable disable

[Keyless] // TODO: AGONY
public partial class FavouriteUserRelation : IRealmObject
{
    public GameUser UserToFavourite { get; set; }
    public GameUser UserFavouriting { get; set; }
}