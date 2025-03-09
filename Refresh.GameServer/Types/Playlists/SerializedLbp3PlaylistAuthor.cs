using System.Xml.Serialization;

[XmlRoot("author")]
[XmlType("author")]
public class SerializedLbp3PlaylistAuthor
{
    public SerializedLbp3PlaylistAuthor() {}
    [XmlElement("npHandle")] public string Username { get; set; } = "";
}