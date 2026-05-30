namespace Refresh.Database.Models.Users;

#nullable disable

public partial class DisallowedEmailAddress
{
    /// <summary>
    /// Lower-case email address to allow case-insensitive lookup.
    /// </summary>
    [Key]
    public string Address { get; set; }
    public string Reason { get; set; }
    public DateTimeOffset DisallowedAt { get; set; }
}