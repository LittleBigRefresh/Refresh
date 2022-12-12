using System.Xml.Serialization;

namespace RefreshTests.HttpServer.Tests;

[XmlRoot("responseSerializedObject")]
public class ResponseSerializationObject
{
    [XmlElement("value")]
    public int Value { get; set; } = 69;
}