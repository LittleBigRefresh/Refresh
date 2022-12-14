using Newtonsoft.Json;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints;

public class AutodiscoverEndpoints : EndpointGroup
{
    public class DiscoverResponse
    {
        private const int CurrentVersion = 1;

        [JsonProperty("version")]
        public int Version { get; set; } = CurrentVersion;

        [JsonProperty("serverBrand")]
        public string ServerBrand { get; set; } = "Refresh";

        // TODO: Configuration option for external url
        [JsonProperty("url")]
        public string Url { get; set; } = "http://127.0.0.1:10061";
    }

    [Endpoint("/autodiscover", ContentType.Json)]
    [Authentication(false)]
    public DiscoverResponse Autodiscover(RequestContext context) => new DiscoverResponse();

}