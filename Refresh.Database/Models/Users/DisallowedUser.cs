namespace Refresh.Database.Models.Users;

#nullable disable

public partial class DisallowedUser
{
    /// <summary>
    /// Lower-case username to allow case-insensitive lookup.
    /// </summary>
    [Key]
    public string Username { get; set; }
    public string Reason { get; set; }
    public DateTimeOffset DisallowedAt { get; set; }
}