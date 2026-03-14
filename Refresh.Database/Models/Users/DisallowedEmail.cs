namespace Refresh.Database.Models.Users;

#nullable disable

public partial class DisallowedEmail
{
    [Key]
    public string Email { get; set; }
}