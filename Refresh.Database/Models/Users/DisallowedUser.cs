namespace Refresh.GameServer.Types.UserData;

public partial class DisallowedUser : IRealmObject
{
    [PrimaryKey]
    public string Username { get; set; }
}