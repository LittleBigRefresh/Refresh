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
    public const string DeletedUserDescription = "Я поддельный пользователь, который представляет удаленных пользователей для уровней.";

    public const string UnknownUserName = "!Unknown";
    public const string UnknownUserDescription = "Я фальшивый пользователь, представляющий несуществующего издателя для переизданных уровней.";
    
    public const string HashedUserName = "!Hashed";
    public const string HashedUserDescription = "Я фальшивый пользователь, представляющий неизвестного издателя хэшированных уровней.";
}