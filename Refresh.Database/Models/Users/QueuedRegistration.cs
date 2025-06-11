using MongoDB.Bson;

#if POSTGRES
using PrimaryKeyAttribute = Refresh.Database.Compatibility.PrimaryKeyAttribute;
#endif

namespace Refresh.Database.Models.Users;

#nullable disable

/// <summary>
/// A registration request waiting to be activated.
/// </summary>
public partial class QueuedRegistration : IRealmObject
{
    [Key, PrimaryKey] public ObjectId RegistrationId { get; set; } = ObjectId.GenerateNewId();
    [Indexed] public string Username { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    [Indexed] public string PasswordBcrypt { get; set; }
    
    public DateTimeOffset ExpiryDate { get; set; }
}