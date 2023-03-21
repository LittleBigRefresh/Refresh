using System.Reflection;
using Bunkum.HttpServer;
using Newtonsoft.Json;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

[JsonObject(MemberSerialization.OptIn)]
public class LevelCategory
{
    private static readonly Lazy<MethodInfo[]> Methods = new(() => typeof(RealmDatabaseContext).GetMethods());
    
    [JsonProperty] public string Name { get; set; } = "";
    [JsonProperty] public string Description { get; set; } = "";
    [JsonProperty] public string IconHash { get; set; } = "0";
    [JsonProperty] public string FontAwesomeIcon { get; set; } = "faCertificate";
    
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

    [JsonProperty] public readonly string ApiRoute;
    public readonly string GameRoute;
    
    [JsonProperty("RequiresUser")] private readonly bool _requiresUser;
    private readonly MethodInfo _method;

    public virtual IEnumerable<GameLevel>? Fetch(RequestContext context, RealmDatabaseContext database, GameUser? user, object[]? extraArgs = null)
    {
        if (this._requiresUser && user == null) return null;
        
        (int skip, int count) = context.GetPageData(context.Url.AbsolutePath.StartsWith("/api"));
        
        IEnumerable<object> args;

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (this._requiresUser)
#pragma warning disable CS8601
            args = new object[] { user, count, skip };
#pragma warning restore CS8601
        else
            args = new object[] { count, skip };

        if (extraArgs != null) args = args.Concat(extraArgs);

        return (IEnumerable<GameLevel>)this._method.Invoke(database, args.ToArray())!;
    }
}