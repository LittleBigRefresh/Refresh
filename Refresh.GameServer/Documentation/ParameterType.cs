using Newtonsoft.Json.Converters;

namespace Refresh.GameServer.Documentation;

[JsonConverter(typeof(StringEnumConverter))]
public enum ParameterType
{
    Route,
    Query,
}