using Refresh.GameServer.Endpoints.ApiV3.DataTypes;

namespace Refresh.GameServer.Endpoints.ApiV3.ApiTypes;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiEmptyResponse : IApiResponse
{}