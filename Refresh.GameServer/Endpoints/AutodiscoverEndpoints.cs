using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Configuration;
using Bunkum.HttpServer.Endpoints;
using Newtonsoft.Json;
using static Refresh.GameServer.Endpoints.GameEndpointAttribute;

namespace Refresh.GameServer.Endpoints;

public class AutodiscoverEndpoints : EndpointGroup
{
    public class DiscoverResponse
    {
        private const int CurrentVersion = 2;

        [JsonProperty("version")]
        public int Version { get; set; } = CurrentVersion;

        [JsonProperty("serverBrand")]
        public string ServerBrand { get; set; } = "Refresh";
        
        [JsonProperty("url")]
        public string Url { get; set; } = "http://127.0.0.1:10061";

        [JsonProperty("usesCustomDigestKey")]
        public bool UsesCustomDigestKey { get; set; } = true;
    }

    [Endpoint("/autodiscover", ContentType.Json)]
    [Authentication(false)]
    public DiscoverResponse Autodiscover(RequestContext context, BunkumConfig config) => new()
    {
        Url = string.Concat(config.ExternalUrl, BaseRoute.AsSpan(0, BaseRoute.Length - 1)),
    };

}