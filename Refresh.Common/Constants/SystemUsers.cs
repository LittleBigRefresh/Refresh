namespace Refresh.Common.Constants;

public static class SystemUsers
{
    /// <summary>
    /// A system user represents a user that doesn't technically exist. (well, it does for now until postgres or something)
    /// This can be used for levels belonging to users who have deleted their accounts, or for re-uploads. 
    /// </summary>
    public const char SystemPrefix = '!';
    /// <summary>
    /// Reserved for clans. https://github.com/LittleBigRefresh/Refresh/issues/77
    /// </summary>
    public const char ClanPrefix = '$';
    
    public const string DeletedUserName = "!DeletedUser";
    public const string DeletedUserDescription = "I'm a fake user that represents deleted users for levels.";

    public const string UnknownUserName = "!Unknown";
    public const string UnknownUserDescription = "I'm a fake user that represents a non existent publisher for re-published levels.";
    
    public const string HashedUserName = "!Hashed";
    public const string HashedUserDescription = "I'm a fake user that represents an unknown publisher for hashed levels.";
}