using System.Net;
using System.Xml.Serialization;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Types;

namespace Refresh.GameServer.Endpoints.Game.Handshake;

public class MetadataEndpoints : EndpointGroup
{
    [GameEndpoint("privacySettings", ContentType.Xml)]
    public PrivacySettings GetPrivacySettings(RequestContext context)
    {
        return new PrivacySettings();
    }
    
    [GameEndpoint("privacySettings", ContentType.Xml, Method.Post)]
    public PrivacySettings SetPrivacySettings(RequestContext context)
    {
        return new PrivacySettings();
    }

    [XmlType("privacySettings")]
    [XmlRoot("privacySettings")]
    public class PrivacySettings
    {
        [XmlElement("levelVisibility")]
        public Visibility LevelVisibility { get; set; } = Visibility.All;
        [XmlElement("profileVisibility")]
        public Visibility ProfileVisibility { get; set; } = Visibility.All;
    }
    
    private static readonly Lazy<string?> NetworkSettingsFile
        = new(() => 
        {
            string path = Path.Combine(Environment.CurrentDirectory, "network_settings.nws");

            return File.Exists(path) ? File.ReadAllText(path) : null;
        });
    
    [GameEndpoint("network_settings.nws")]
    public string? NetworkSettings(RequestContext context)
    {
        bool created = NetworkSettingsFile.IsValueCreated;
        
        string? networkSettings = NetworkSettingsFile.Value;
        
        // Only log this warning once
        if(!created && networkSettings == null)
            context.Logger.LogWarning(BunkumContext.Request, "network_settings.nws file is missing! " +
                                                              "LBP will work without it, but it may be relevant to you if you are an advanced user.");
        
        return networkSettings;
    }
    
    private static readonly Lazy<string?> TelemetryConfigFile
        = new(() => 
        {
            string path = Path.Combine(Environment.CurrentDirectory, "telemetry.xml");

            return File.Exists(path) ? File.ReadAllText(path) : null;
        });

    [GameEndpoint("t_conf")]
    [NullStatusCode(HttpStatusCode.Gone)]
    public string? TelemetryConfig(RequestContext context) 
    {
        bool created = TelemetryConfigFile.IsValueCreated;
        
        string? telemetryConfig = TelemetryConfigFile.Value;
        
        // Only log this warning once
        if (!created && telemetryConfig == null)
            context.Logger.LogWarning(BunkumContext.Request, "telemetry.xml file is missing! " +
                                                             "LBP will work without it, but it may be relevant to you if you are an advanced user.");

        return telemetryConfig;
    }
    
    private static readonly Lazy<string?> PromotionsFile
        = new(() => 
        {
            string path = Path.Combine(Environment.CurrentDirectory, "promotions.xml");

            return File.Exists(path) ? File.ReadAllText(path) : null;
        });

    [GameEndpoint("promotions")]
    [NullStatusCode(HttpStatusCode.OK)]
    public string? Promotions(RequestContext context) 
    {
        bool created = PromotionsFile.IsValueCreated;
        
        string? promotions = PromotionsFile.Value;
        
        // Only log this warning once
        if(!created && promotions == null)
            context.Logger.LogWarning(BunkumContext.Request, "promotions.xml file is missing! " +
                                                             "LBP will work without it, but it may be relevant to you if you are an advanced user.");
        
        return promotions;
    }
}