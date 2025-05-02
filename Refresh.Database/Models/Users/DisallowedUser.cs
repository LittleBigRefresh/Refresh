namespace Refresh.Database.Models.Users;

public partial class DisallowedUser : IRealmObject
{
    [PrimaryKey]
    public string Username { get; set; }
}