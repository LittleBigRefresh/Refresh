using System.Security.Cryptography;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer.Tests.Assets;

public class AssetUploadTests : GameServerTest
{
    [Test]
    public void CanUploadAsset()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddService<ImportService>();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        ReadOnlySpan<byte> data = "TEX a"u8;
        
        string hash = BitConverter.ToString(SHA1.HashData(data))
            .Replace("-", "")
            .ToLower();

        HttpResponseMessage response = client.PostAsync("/lbp/upload/" + hash, new ByteArrayContent(data.ToArray())).Result;
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    public void CanRetrieveAsset()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddService<ImportService>();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        ReadOnlySpan<byte> data = "TEX a"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data))
            .Replace("-", "")
            .ToLower();

        client.PostAsync("/lbp/upload/" + hash, new ByteArrayContent(data.ToArray())).Wait();
        HttpResponseMessage response = client.GetAsync("/lbp/r/" + hash).Result;
        Assert.That(response.IsSuccessStatusCode, Is.True);
        byte[] returnedData = response.Content.ReadAsByteArrayAsync().Result;
        
        Assert.That(data.SequenceEqual(returnedData), Is.True);
    }

    [Test]
    public void InvalidHashFails()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddService<ImportService>();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        HttpResponseMessage response = client.GetAsync("/lbp/r/asdf").Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
        
        response = client.GetAsync("/lbp/r/..%2Frpc.json").Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }
}