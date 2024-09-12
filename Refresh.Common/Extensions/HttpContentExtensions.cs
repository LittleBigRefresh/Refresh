using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Refresh.Common.Extensions;

public static class HttpContentExtensions
{
    public static T ReadAsXml<T>(this HttpContent content)
    {
        XmlSerializer serializer = new(typeof(T));

        return (T)serializer.Deserialize(new XmlTextReader(content.ReadAsStream()))!;
    }
    
    public static T? ReadAsJson<T>(this HttpContent content)
    {
        return JsonConvert.DeserializeObject<T>(content.ReadAsStringAsync().Result);
    }
}