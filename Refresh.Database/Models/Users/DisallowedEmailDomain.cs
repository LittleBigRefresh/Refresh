namespace Refresh.Database.Models.Users;

#nullable disable

public partial class DisallowedEmailDomain
{
    /// <summary>
    /// Lower-case email domain to allow case-insensitive lookup.
    /// </summary>
    [Key]
    public string Domain { get; set; }
    public string Reason { get; set; }
    public DateTimeOffset DisallowedAt { get; set; }
}