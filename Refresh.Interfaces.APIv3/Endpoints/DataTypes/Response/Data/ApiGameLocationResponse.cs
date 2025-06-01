namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Data;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameLocationResponse : IApiResponse
{
    public required int X { get; set; }
    public required int Y { get; set; }
    public static ApiGameLocationResponse? FromLocation(int x, int y)
    {
        return new ApiGameLocationResponse
        {
            X = x,
            Y = y,
        };
    }
}