namespace Refresh.Database.Models.Users;

#if POSTGRES
using PrimaryKeyAttribute = Refresh.Database.Compatibility.PrimaryKeyAttribute;
#endif

#nullable disable

public partial class DisallowedUser : IRealmObject
{
    [Key, PrimaryKey]
    public string Username { get; set; }
}