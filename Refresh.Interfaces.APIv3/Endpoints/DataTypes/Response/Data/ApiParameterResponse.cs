using AttribDoc;
using Newtonsoft.Json.Converters;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Data;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiParameterResponse : IApiResponse
{
    public required string Name { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public required ParameterType Type { get; set; }
    public required string Summary { get; set; }
    
    public static ApiParameterResponse? FromOld(Parameter? old)
    {
        if (old == null) return null;

        return new ApiParameterResponse
        {
            Name = old.Name,
            Summary = old.Summary,
            Type = old.Type,
        };
    }

    public static IEnumerable<ApiParameterResponse> FromOldList(IEnumerable<Parameter> oldList) => oldList.Select(FromOld).ToList()!;
}