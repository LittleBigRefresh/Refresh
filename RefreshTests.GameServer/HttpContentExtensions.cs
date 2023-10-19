using System.Xml;
using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;

namespace RefreshTests.GameServer;

public static class HttpContentExtensions
{
    public static T ReadAsXML<T>(this HttpContent content)
    {
        XmlSerializer serializer = new(typeof(T));

        return (T)serializer.Deserialize(new XmlTextReader(content.ReadAsStream()))!;
    }
}