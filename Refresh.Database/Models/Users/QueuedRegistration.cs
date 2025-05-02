using MongoDB.Bson;
using Realms;

namespace Refresh.GameServer.Types.UserData;

#nullable disable

/// <summary>
/// A registration request waiting to be activated.
/// </summary>
public partial class QueuedRegistration : IRealmObject
{
    [PrimaryKey] public ObjectId RegistrationId { get; set; } = ObjectId.GenerateNewId();
    [Indexed] public string Username { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    [Indexed] public string PasswordBcrypt { get; set; }
    
    public DateTimeOffset ExpiryDate { get; set; }
}