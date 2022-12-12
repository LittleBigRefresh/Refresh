using System.Net;
using System.Text;
using System.Xml.Serialization;
using Refresh.HttpServer;
using RefreshTests.HttpServer.Endpoints;

namespace RefreshTests.HttpServer.Tests;

public class ResponseTests : ServerDependentTest
{
    [Test]
    [TestCase("/response/string", HttpStatusCode.OK)]
    [TestCase("/response/responseObject", HttpStatusCode.OK)]
    [TestCase("/response/responseObjectWithCode", HttpStatusCode.Accepted)]
    public void CorrectResponseForAllTypes(string endpoint, HttpStatusCode codeToCheckFor)
    {
        (RefreshHttpServer? server, HttpClient? client) = this.Setup();
        server.AddEndpointGroup<ResponseEndpoints>();
        
        HttpResponseMessage msg = client.Send(new HttpRequestMessage(HttpMethod.Get, endpoint));
        Assert.Multiple(async () =>
        {
            Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo("works"));
            Assert.That(msg.StatusCode, Is.EqualTo(codeToCheckFor));
        });
    }

    [Test]
    [TestCase("/response/serializedXml")]
    // [TestCase("/response/serializedJson")]
    public async Task CorrectResponseForSerializedObject(string endpoint)
    {
        (RefreshHttpServer? server, HttpClient? client) = this.Setup();
        server.AddEndpointGroup<ResponseEndpoints>();
        
        HttpResponseMessage msg = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, endpoint));
        string text = await msg.Content.ReadAsStringAsync();
        
        Assert.That(text, Is.EqualTo("<responseSerializedObject><value>69</value></responseSerializedObject>"));
        
        // technically we probably dont need to test deserialization but we should anyways
        XmlSerializer serializer = new(typeof(ResponseSerializationObject));

        ResponseSerializationObject? obj = (ResponseSerializationObject?)serializer.Deserialize(new MemoryStream(Encoding.Default.GetBytes(text)));
        Assert.That(obj, Is.Not.Null);
        Assert.That(obj!.Value, Is.EqualTo(69));
    }
}