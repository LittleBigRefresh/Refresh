using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiError
{
    public ApiError(string message)
    {
        this.Message = message;
    }

    public string Message { get; set; }
}