using System.Diagnostics.CodeAnalysis;

namespace Refresh.GameServer.Types.Matching.Responses;

[JsonObject(MemberSerialization.OptIn)]
public class SerializedStatusCodeMatchResponse
{
    [ExcludeFromCodeCoverage]
    public SerializedStatusCodeMatchResponse() {}

    public SerializedStatusCodeMatchResponse(int statusCode)
    {
        this.StatusCode = statusCode;
    }

    [JsonProperty] public int StatusCode { get; set; }
}