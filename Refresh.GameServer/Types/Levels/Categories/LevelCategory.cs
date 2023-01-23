using System.Reflection;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class LevelCategory
{
    private static readonly Lazy<MethodInfo[]> Methods = new(() => typeof(RealmDatabaseContext).GetMethods());
    
    internal LevelCategory(string apiRoute, string gameRoute, bool requiresUser, string funcName)
    {
        this.ApiRoute = apiRoute;
        this.GameRoute = gameRoute;
        
        this._requiresUser = requiresUser;

        MethodInfo? method = Methods.Value.FirstOrDefault(m => m.Name == funcName);
        if (method == null) throw new ArgumentNullException(nameof(funcName), 
            $"{nameof(funcName)} must point to a method on {nameof(RealmDatabaseContext)}! Use nameof() to assist with this.");

        this._method = method;
    }

    public readonly string ApiRoute;
    public readonly string GameRoute;
    
    private readonly bool _requiresUser;
    private readonly MethodInfo _method;

    public IEnumerable<GameLevel>? Fetch(RealmDatabaseContext database, GameUser? user, int count, int skip)
    {
        if (this._requiresUser && user == null) return null;
        
        object[] args;

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (this._requiresUser)
#pragma warning disable CS8601
            args = new object[] { user, count, skip };
#pragma warning restore CS8601
        else
            args = new object[] { count, skip };

        return (IEnumerable<GameLevel>)this._method.Invoke(database, args)!;
    }
}