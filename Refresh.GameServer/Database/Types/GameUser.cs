using MongoDB.Bson;
using Realms;
using Refresh.HttpServer.Authentication;

namespace Refresh.GameServer.Database.Types;

public class GameUser : RealmObject, IUser
{
    [PrimaryKey] [Indexed] public ObjectId UserId { get; set; } = ObjectId.GenerateNewId();
    [Indexed] [Required] public string Username { get; set; }
}