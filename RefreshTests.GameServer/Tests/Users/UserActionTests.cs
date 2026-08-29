using System.Net;
using System.Security.Cryptography;
using Refresh.Common.Constants;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.UserData;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Users;

public class UserActionTests : GameServerTest
{
    [Test]
    public void RenamesUser()
    {
        using TestContext context = this.GetServer(false);
        GameUser? user = context.CreateUser("gamer1");
        
        Assert.That(user.Username, Is.EqualTo("gamer1"));
        
        context.Database.RenameUser(user, "gamer2");
        user = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(user, Is.Not.Null);
        
        Assert.That(user!.Username, Is.EqualTo("gamer2"));
    }

    [Test]
    public void UserDescriptionGetsTrimmed()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedUpdateDataProfile request = new()
        {
            Description = new string('S', 600),
        };

        HttpResponseMessage response = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();

        // Ensure the description was trimmed
        GameUser? updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Description.Length, Is.EqualTo(UgcLimits.DescriptionLimit));
    }

    [Test]
    public void CanSetAndResetOwnIcon()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        // Set to GUID (star sticker here)
        SerializedUpdateDataPlanets request = new()
        {
            IconHash = "g18451"
        };
        HttpResponseMessage response = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();
        GameUser? userUpdated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(userUpdated, Is.Not.Null);
        Assert.That(userUpdated!.IconHash, Is.EqualTo("g18451"));

        // Set to hash
        ReadOnlySpan<byte> data = "TEX "u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);

        request.IconHash = hash;
        response = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(response, Is.Not.Null);

        context.Database.Refresh();
        userUpdated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(userUpdated, Is.Not.Null);
        Assert.That(userUpdated!.IconHash, Is.EqualTo(hash));

        // Now reset
        request.IconHash = "";
        response = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(response, Is.Not.Null);

        context.Database.Refresh();
        userUpdated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(userUpdated, Is.Not.Null);
        Assert.That(userUpdated!.IconHash, Is.EqualTo("0"));
    }

    [Test]
    public void RejectIconIfMissing()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "TEX "u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        // Don't upload asset

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        SerializedUpdateDataPlanets request = new()
        {
            IconHash = hash
        };
        HttpResponseMessage response = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(NotFound));
    }

    [Test]
    public void RejectIconIfInvalidGuid()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        SerializedUpdateDataPlanets request = new()
        {
            IconHash = "g1087",
        };
        HttpResponseMessage response = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }

    [Test]
    public void RejectDisallowedIcon()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "TEX "u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);
        context.Database.DisallowAsset(hash, GameAssetType.Texture, "ugly");

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        SerializedUpdateDataPlanets request = new()
        {
            IconHash = hash,
        };
        HttpResponseMessage response = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(Unauthorized));
    }

    [Test]
    public void AcceptValidFaceIcons()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> yayData = "TEX yay"u8;
        string yayHash = BitConverter.ToString(SHA1.HashData(yayData)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(yayHash, yayData);

        ReadOnlySpan<byte> mehData = "TEX meh"u8;
        string mehHash = BitConverter.ToString(SHA1.HashData(mehData)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(mehHash, mehData);

        ReadOnlySpan<byte> booData = "TEX boo"u8;
        string booHash = BitConverter.ToString(SHA1.HashData(booData)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(booHash, booData);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        // update all of them
        SerializedUpdateDataPlanets request = new()
        {
            YayFaceHash = yayHash,
            MehFaceHash = mehHash,
            BooFaceHash = booHash,
        };
        HttpResponseMessage response = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
    }
    
    private void TryUpdateProfileWithInvalidFaceIcon(TestContext context, HttpClient client, string invalidHash, HttpStatusCode expectedStatus)
    {
        ReadOnlySpan<byte> yayData = "TEX yay"u8;
        string yayHash = BitConverter.ToString(SHA1.HashData(yayData)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(yayHash, yayData);

        ReadOnlySpan<byte> mehData = "TEX meh"u8;
        string mehHash = BitConverter.ToString(SHA1.HashData(mehData)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(mehHash, mehData);

        ReadOnlySpan<byte> booData = "TEX boo"u8;
        string booHash = BitConverter.ToString(SHA1.HashData(booData)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(booHash, booData);

        // bad yay icon
        SerializedUpdateDataPlanets request = new()
        {
            YayFaceHash = invalidHash,
            MehFaceHash = mehHash,
            BooFaceHash = booHash,
        };
        
        HttpResponseMessage response = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(expectedStatus));

        // bad meh icon
        request.YayFaceHash = yayHash;
        request.MehFaceHash = invalidHash;

        response = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(expectedStatus));

        // bad boo icon
        request.MehFaceHash = mehHash;
        request.BooFaceHash = invalidHash;

        response = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(expectedStatus));
    }

    [Test]
    [TestCase("0")]
    [TestCase("")]
    [TestCase("g18451")] // star sticker texture
    [TestCase("g1087")] // sackboy model
    [TestCase("INVALID HASH!!!")]
    public void RejectFaceIconIfNotHash(string assetRef)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        this.TryUpdateProfileWithInvalidFaceIcon(context, client, assetRef, BadRequest);
    }

    [Test]
    public void RejectFaceIconIfMissing()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        ReadOnlySpan<byte> badData = "TEX reject"u8;
        string badHash = BitConverter.ToString(SHA1.HashData(badData)).Replace("-", "").ToLower();
        // Don't upload asset

        this.TryUpdateProfileWithInvalidFaceIcon(context, client, badHash, NotFound);
    }

    [Test]
    public void RejectFaceIconIfDisallowed()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        ReadOnlySpan<byte> badData = "TEX d"u8;
        string badHash = BitConverter.ToString(SHA1.HashData(badData)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(badHash, badData);
        context.Database.DisallowAsset(badHash, GameAssetType.Texture, "");

       this.TryUpdateProfileWithInvalidFaceIcon(context, client, badHash, Unauthorized);
    }

    [Test]
    public void DeletingUserDoesNotDisallowEmail()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        Assert.That(publisher.EmailAddress, Is.Not.Null);
        Assert.That(context.Database.IsEmailAddressDisallowed(publisher.EmailAddress!), Is.False);
        string email = publisher.EmailAddress!;

        // Delete publisher
        context.Database.DeleteUser(publisher);
        context.Database.Refresh();
        
        Assert.That(context.Database.IsEmailAddressDisallowed(email), Is.False);
    }
}