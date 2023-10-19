using System.Xml.Serialization;
using Newtonsoft.Json;

namespace RefreshTests.GameServer.Extensions;

public static class ObjectExtensions
{
    public static string AsXML(this object obj)
    {
        XmlSerializer serializer = new(obj.GetType());

        TextWriter writer = new StringWriter();
        serializer.Serialize(writer, obj);

        return writer.ToString()!;
    }
    
    public static string AsJson(this object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }
}