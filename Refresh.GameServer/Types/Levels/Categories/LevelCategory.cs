using System.Diagnostics.Contracts;
using System.Reflection;
using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

[JsonObject(MemberSerialization.OptIn)]
public class LevelCategory
{
    private static readonly Lazy<MethodInfo[]> Methods = new(() => typeof(GameDatabaseContext).GetMethods());
    
    [JsonProperty] public string Name { get; set; } = "";
    [JsonProperty] public string Description { get; set; } = "";
    [JsonProperty] public string IconHash { get; set; } = "0";
    [JsonProperty] public string FontAwesomeIcon { get; set; } = "faCertificate";
    
    #if DEBUG
    private static readonly List<string> ExpectedParameters = new()
    {
        "count",
        "skip",
        "gameVersion",
    };
    #endif
    
    internal LevelCategory(string apiRoute, string gameRoute, bool requiresUser, string funcName) : this(apiRoute, new []{gameRoute}, requiresUser, funcName) {}
    
    internal LevelCategory(string apiRoute, string[] gameRoutes, bool requiresUser, string funcName)
    {
        this.ApiRoute = apiRoute;
        this.GameRoutes = gameRoutes;
        
        this.RequiresUser = requiresUser;

        MethodInfo? method = Methods.Value.FirstOrDefault(m => m.Name == funcName);
        if (method == null) throw new ArgumentNullException(nameof(funcName), 
            $"{nameof(funcName)} must point to a method on {nameof(GameDatabaseContext)}! Use nameof() to assist with this.");

        #if DEBUG
        List<string> parameters = method.GetParameters().Select(p => p.Name).ToList()!;
        foreach (string expectedParameter in ExpectedParameters)
        {
            if (parameters.Contains(expectedParameter)) continue;
            throw new InvalidOperationException($"Cannot bind to {funcName}() when it is missing a {expectedParameter} parameter");
        }
        #endif

        this._method = method;
    }

    [JsonProperty] public readonly string ApiRoute;
    public readonly string[] GameRoutes;
    
    [JsonProperty] public readonly bool RequiresUser;
    private readonly MethodInfo _method;

    [Pure]
    public virtual DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, MatchService matchService, GameDatabaseContext database, GameUser? user, TokenGame gameVersion, object[]? extraArgs = null)
    {
        if (this.RequiresUser && user == null) return null;

        IEnumerable<object> args;

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (this.RequiresUser)
#pragma warning disable CS8601
            args = new object[] { user, count, skip, gameVersion };
#pragma warning restore CS8601
        else
            args = new object[] { count, skip, gameVersion };

        if (extraArgs != null) args = args.Concat(extraArgs);

        try
        {
            return (DatabaseList<GameLevel>)this._method.Invoke(database, args.ToArray())!;
        }
        catch
        {
            Console.WriteLine(this.ApiRoute);
            throw;
        }
    }
}