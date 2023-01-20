using System.Xml.Serialization;
using Refresh.GameServer.Types;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints.Handshake;

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
}