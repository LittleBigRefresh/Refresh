using JetBrains.Annotations;
using Refresh.GameServer.Types;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Data;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameLocationResponse : IApiResponse
{
    public required int X { get; set; }
    public required int Y { get; set; }

    [ContractAnnotation("null => null; notnull => notnull")]
    public static ApiGameLocationResponse? FromGameLocation(GameLocation? location)
    {
        if (location == null) return null;
        
        return new ApiGameLocationResponse
        {
            X = location.X,
            Y = location.Y,
        };
    }
}