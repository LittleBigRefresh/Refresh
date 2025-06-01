using Refresh.Interfaces.APIv3.Endpoints.DataTypes;

namespace Refresh.Interfaces.APIv3.Endpoints.ApiTypes;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiEmptyResponse : IApiResponse
{}