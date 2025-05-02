using Realms;

namespace Refresh.GameServer.Types.UserData;

#nullable disable

public partial class EmailVerificationCode : IRealmObject
{
    public GameUser User { get; set; }
    public string Code { get; set; }
    
    public DateTimeOffset ExpiryDate { get; set; }
}