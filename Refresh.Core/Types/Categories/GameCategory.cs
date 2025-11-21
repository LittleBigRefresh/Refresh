using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories;

[JsonObject(MemberSerialization.OptIn)]
public abstract class GameCategory
{
    [JsonProperty] public string Name { get; set; } = "";
    [JsonProperty] public string Description { get; set; } = "";
    [JsonProperty] public string IconHash { get; set; } = "0";
    [JsonProperty] public string FontAwesomeIcon { get; set; } = "faCertificate";
    [JsonProperty] public bool Hidden { get; set; } = false;
    public ResultType PrimaryResultType { get; set; }

    [JsonProperty] public readonly bool RequiresUser;
    [JsonProperty] public readonly string ApiRoute;
    public readonly string[] GameRoutes;

    internal GameCategory(string apiRoute, string gameRoute, bool requiresUser) : this(apiRoute, [gameRoute], requiresUser) {}
    internal GameCategory(string apiRoute, string[] gameRoutes, bool requiresUser)
    {
        this.ApiRoute = apiRoute;
        this.GameRoutes = gameRoutes;
        
        this.RequiresUser = requiresUser;
    }

    public abstract DatabaseResultList? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? user);
}