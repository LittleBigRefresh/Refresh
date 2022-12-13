using System.Xml.Serialization;
using Newtonsoft.Json;

namespace RefreshTests.HttpServer.Tests;

[XmlRoot("responseSerializedObject")]
public class ResponseSerializationObject
{
    [XmlElement("value")]
    [JsonProperty("value")]
    public int Value { get; set; } = 69;
}