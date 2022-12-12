using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Refresh.HttpServer.Responses;

public struct Response
{
    public Response(byte[] data, ContentType contentType = ContentType.Html, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        this.StatusCode = statusCode;
        this.Data = data;
        this.ContentType = contentType;
    }

    #region XML Serialization setup
    private static readonly XmlWriterSettings WriterSettings = new()
    {
        ConformanceLevel = ConformanceLevel.Document,
        OmitXmlDeclaration = true,
    };

    private static readonly XmlSerializerNamespaces Namespaces = new();

    static Response()
    {
        Namespaces.Add("", "");
    }
    #endregion

    public Response(object? data, ContentType contentType = ContentType.Html, HttpStatusCode statusCode = HttpStatusCode.OK, bool skipSerialization = false)
    {
        this.ContentType = contentType;
        this.StatusCode = statusCode;

        if (skipSerialization || data == null || !contentType.IsSerializable())
        {
            this.Data = Encoding.Default.GetBytes(data?.ToString() ?? string.Empty);
            return;
        }

        MemoryStream stream = new();
        switch (contentType)
        {
            case ContentType.Html:
            case ContentType.Plaintext:
                throw new InvalidOperationException();
            case ContentType.Xml:
                XmlWriter writer = XmlWriter.Create(stream, WriterSettings);

                XmlSerializer serializer = new(data.GetType());
                serializer.Serialize(writer, data, Namespaces);
                break;
            case ContentType.Json:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(contentType), contentType, null);
        }

        this.Data = stream.ToArray();
    }

    public readonly HttpStatusCode StatusCode;
    public readonly ContentType ContentType;
    public readonly byte[] Data;
}