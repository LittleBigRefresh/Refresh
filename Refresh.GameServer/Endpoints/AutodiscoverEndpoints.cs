using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Configuration;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Newtonsoft.Json;

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
        
        [JsonProperty("url")]
        public string Url { get; set; } = "http://127.0.0.1:10061";
    }

    [Endpoint("/autodiscover", ContentType.Json)]
    [Authentication(false)]
    public DiscoverResponse Autodiscover(RequestContext context, BunkumConfig config) => new()
    {
        Url = config.ExternalUrl,
    };

}