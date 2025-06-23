using MongoDB.Bson;

namespace Refresh.Database.Models.Users;

#nullable disable

/// <summary>
/// A registration request waiting to be activated.
/// </summary>
[Index(nameof(Username), nameof(EmailAddress))]
public partial class QueuedRegistration
{
    [Key] public ObjectId RegistrationId { get; set; } = ObjectId.GenerateNewId();
    public string Username { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string PasswordBcrypt { get; set; }
    
    public DateTimeOffset ExpiryDate { get; set; }
}