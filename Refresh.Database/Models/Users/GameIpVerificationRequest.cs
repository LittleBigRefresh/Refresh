namespace Refresh.Database.Models.Users;

#nullable disable

public partial class GameIpVerificationRequest : IRealmObject
{
    public GameUser User { get; set; }
    
    public string IpAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}