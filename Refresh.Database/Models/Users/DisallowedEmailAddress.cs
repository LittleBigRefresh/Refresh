namespace Refresh.Database.Models.Users;

#nullable disable

public partial class DisallowedEmailAddress
{
    [Key]
    public string Address { get; set; }
    public string Reason { get; set; }
    public DateTimeOffset DisallowedAt { get; set; }
}