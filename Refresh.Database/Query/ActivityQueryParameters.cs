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

    /// <summary>
    /// The requesting user
    /// </summary>
    public GameUser? User = null;
    
    // The defaults for the 3 params below must stay this way, to not risk breaking 
    // something in-game and also accidentally leaking mod events on Discord
    public bool IncludeActivity = true;
    public bool IncludeDeletedActivity = false;
    public bool IncludeModeration = false;
    public bool IsGameRequest = false;

    public ActivityQueryParameters()
    {}
}