using System.Xml.Serialization;

namespace RefreshTests.GameServer;

public static class ObjectExtensions
{
    public static string AsXML(this object obj)
    {
        XmlSerializer serializer = new(obj.GetType());

        TextWriter writer = new StringWriter();
        serializer.Serialize(writer, obj);

        return writer.ToString()!;
    }
}