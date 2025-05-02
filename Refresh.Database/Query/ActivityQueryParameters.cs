using Refresh.Database.Models.Users;

namespace Refresh.Database.Query;

public struct ActivityQueryParameters
{
    public int Count = 20;
    public int Skip = 0;
    public long Timestamp = 0;
    public long EndTimestamp = 0;
    
    public bool ExcludeMyLevels = false;
    public bool ExcludeFriends = false;
    public bool ExcludeFavouriteUsers = false;
    public bool ExcludeMyself = false;

    public GameUser? User = null;

    public ActivityQueryParameters()
    {}
}