namespace Refresh.Database.Models.Users;

#nullable disable

public partial class DisallowedUser
{
    [Key]
    public string Username { get; set; }
    public string Reason { get; set; }
    public DateTimeOffset DisallowedAt { get; set; }
}