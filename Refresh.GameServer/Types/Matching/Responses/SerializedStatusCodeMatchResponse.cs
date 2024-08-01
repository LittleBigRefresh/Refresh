using System.Diagnostics.CodeAnalysis;
using System.Net;
using Bunkum.Core.Responses;

namespace Refresh.GameServer.Types.Matching.Responses;

[JsonObject(MemberSerialization.OptIn)]
public class SerializedStatusCodeMatchResponse: IHasResponseCode
{
    [ExcludeFromCodeCoverage]
    public SerializedStatusCodeMatchResponse() {}

    public SerializedStatusCodeMatchResponse(HttpStatusCode statusCode)
    {
        this.StatusCode = statusCode;
    }

    [JsonProperty] public HttpStatusCode StatusCode { get; set; }
}