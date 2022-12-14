using System.Text;
using System.Xml;

namespace Refresh.HttpServer.Serialization;

public class RefreshXmlTextWriter : XmlTextWriter
{
    public RefreshXmlTextWriter(Stream stream) : base(stream, Encoding.UTF8)
    { }

    public override void WriteEndElement()
    {
        base.WriteFullEndElement();
    }

    public override Task WriteEndElementAsync()
    {
        return base.WriteFullEndElementAsync();
    }

    public override void WriteStartDocument() {}
    public override void WriteStartDocument(bool standalone) {}
}