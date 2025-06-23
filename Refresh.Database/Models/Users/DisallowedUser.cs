namespace Refresh.Database.Models.Users;

#nullable disable

public partial class DisallowedUser
{
    [Key]
    public string Username { get; set; }
}