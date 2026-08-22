using System.Security.Cryptography;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.UserData;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Planets;

public class PlanetUploadTests : GameServerTest
{
    [Test]
    public void RejectPlanetsIfGuid()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        SerializedUpdateDataProfile request = new()
        {
            PlanetsHash = "g34567",
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }

    [Test]
    public void AllowBlankPlanets()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        SerializedUpdateDataProfile request = new()
        {
            PlanetsHash = "0",
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    public void AllowHashedPlanets()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "LVLb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        SerializedUpdateDataProfile request = new()
        {
            PlanetsHash = hash,
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    public void AllowHashedPlanetsWithNewline()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "LVLb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        SerializedUpdateDataProfile request = new()
        {
            PlanetsHash = hash + "\n",
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    public void RejectPlanetsIfWrongResourceType()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "PLNb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        SerializedUpdateDataProfile request = new()
        {
            PlanetsHash = hash,
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }

    [Test]
    public void RejectPlanetsIfNotUploaded()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "LVLb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        // Don't upload the asset

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        SerializedUpdateDataProfile request = new()
        {
            PlanetsHash = hash,
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }

    [Test]
    public void RejectPlanetsIfDisallowed()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "LVLb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);
        context.Database.DisallowAsset(hash, GameAssetType.Level, "garbage music");

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        SerializedUpdateDataProfile request = new()
        {
            PlanetsHash = hash,
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
    }

    [Test]
    public void RejectPlanetsIfInvalidHash()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        SerializedUpdateDataProfile request = new()
        {
            PlanetsHash = "adserdtfgzhgj",
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
}