namespace Refresh.Database.Models.Users;

#nullable disable

public partial class DisallowedEmailAddress
{
    [Key]
    public string Address { get; set; }
}