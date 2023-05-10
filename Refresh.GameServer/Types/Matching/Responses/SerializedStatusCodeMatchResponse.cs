using Newtonsoft.Json;

namespace Refresh.GameServer.Types.Matching.Responses;

[JsonObject(MemberSerialization.OptIn)]
public class SerializedStatusCodeMatchResponse
{
    public SerializedStatusCodeMatchResponse() {}

    public SerializedStatusCodeMatchResponse(int statusCode)
    {
        this.StatusCode = statusCode;
    }

    [JsonProperty] public int StatusCode { get; set; }
}