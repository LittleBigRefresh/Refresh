using System.Diagnostics.Contracts;
using System.Reflection;
using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

[JsonObject(MemberSerialization.OptIn)]
public abstract class LevelCategory
{
    [JsonProperty] public string Name { get; set; } = "";
    [JsonProperty] public string Description { get; set; } = "";
    [JsonProperty] public string IconHash { get; set; } = "0";
    [JsonProperty] public string FontAwesomeIcon { get; set; } = "faCertificate";
    
    internal LevelCategory(string apiRoute, string gameRoute, bool requiresUser) : this(apiRoute, new []{gameRoute}, requiresUser) {}
    
    internal LevelCategory(string apiRoute, string[] gameRoutes, bool requiresUser)
    {
        this.ApiRoute = apiRoute;
        this.GameRoutes = gameRoutes;
        
        this.RequiresUser = requiresUser;
    }

    [JsonProperty] public readonly string ApiRoute;
    public readonly string[] GameRoutes;
    
    [JsonProperty] public readonly bool RequiresUser;

    [Pure]
    public abstract DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, MatchService matchService, GameDatabaseContext database, GameUser? user, TokenGame gameVersion,
        LevelFilterSettings levelFilterSettings);
}