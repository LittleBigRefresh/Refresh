using Realms;
using Refresh.GameServer.Configuration;

namespace Refresh.GameServer.Types.UserData;

public partial class LevelUploads : IRealmObject
{
    public int Count { get; set; }

    private bool DateIsExpired { get; set; }

    private DateTimeOffset ExpiryDate { get; set; }

    private LevelUploads()
    {
        this.Count = 0;
        this.DateIsExpired = true;
        this.ExpiryDate = DateTimeOffset.Now;
    }

    public void IncrementUploadCount(GameServerConfig config)
    {
        // Set ExpiryDate if the user has uploaded their first level within the configured length of time
        if (this.Count == 0)
        {
            this.ExpiryDate = DateTime.Now + TimeSpan.FromHours(config.LevelUploadTimeSpan);
            this.DateIsExpired = false;
        }

        this.Count += 1;
    }

    public void TryExpireUploadCount()
    {
        if (this.ExpiryDate >= DateTime.Now) return;
        
        this.DateIsExpired = true;
        this.Count = 0;
    }
}