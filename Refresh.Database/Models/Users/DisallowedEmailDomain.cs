namespace Refresh.Database.Models.Users;

#nullable disable

public partial class DisallowedEmailDomain
{
    [Key]
    public string Domain { get; set; }
}