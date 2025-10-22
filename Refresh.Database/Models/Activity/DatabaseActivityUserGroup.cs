using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Activity;

public class DatabaseActivityUserGroup : DatabaseActivityGroup
{
    public DatabaseActivityUserGroup(GameUser? user, ObjectId userId)
    {
        this.User = user;
        this.UserId = userId;
    }

    public override string GroupType => "user";
    public GameUser? User { get; set; }
    public ObjectId UserId { get; set; }
}