using Refresh.Database.Models;
using Refresh.Database.Query;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiPlaylistCreationRequest : ISerializedCreatePlaylistInfo
{
    public string? Name { get; set; }
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public GameLocation? Location { get; set; }
}