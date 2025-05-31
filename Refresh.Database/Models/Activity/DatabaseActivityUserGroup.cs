using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Activity;

public class DatabaseActivityUserGroup : DatabaseActivityGroup
{
    public DatabaseActivityUserGroup(GameUser user)
    {
        this.User = user;
    }

    public override string GroupType => "user";
    public GameUser User { get; set; }
}