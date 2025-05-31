namespace Refresh.Database.Models.Users;

#nullable disable

public partial class DisallowedUser : IRealmObject
{
    [PrimaryKey]
    public string Username { get; set; }
}