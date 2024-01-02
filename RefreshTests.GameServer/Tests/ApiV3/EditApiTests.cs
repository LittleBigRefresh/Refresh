using System.Net.Http.Json;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class EditApiTests : GameServerTest
{
    [Test]
    public void CanUpdateLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user, "Not updated");

        long oldUpdate = level.UpdateDate;

        ApiEditLevelRequest payload = new()
        {
            Title = "Updated",
        };

        context.Time.TimestampMilliseconds = 1;
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        HttpResponseMessage response = client.PatchAsync($"/api/v3/levels/id/{level.LevelId}", JsonContent.Create(payload)).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();
        Assert.Multiple(() =>
        {
            Assert.That(level.Title, Is.EqualTo("Updated"));
            Assert.That(level.UpdateDate, Is.Not.EqualTo(oldUpdate));
            Assert.That(level.UpdateDate, Is.EqualTo(context.Time.TimestampMilliseconds));
        });
    }
    
    [Test]
    public void OtherUserCantUpdateLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user, "Not updated");

        ApiEditLevelRequest payload = new()
        {
            Title = "Updated",
        };
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user2);
        HttpResponseMessage response = client.PatchAsync($"/api/v3/levels/id/{level.LevelId}", JsonContent.Create(payload)).Result;
        Assert.That(response.StatusCode, Is.EqualTo(Forbidden));
        
        context.Database.Refresh();
        Assert.Multiple(() =>
        {
            Assert.That(level.Title, Is.EqualTo("Not updated"));
        });
    }

    [Test]
    public void AdminCanUpdateLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameUser admin = context.CreateAdmin();
        GameLevel level = context.CreateLevel(user, "Not updated");

        long oldUpdate = level.UpdateDate;

        ApiAdminEditLevelRequest payload = new()
        {
            Title = "Updated",
            GameVersion = TokenGame.LittleBigPlanetPSP,
        };

        context.Time.TimestampMilliseconds = 1;
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, admin);
        HttpResponseMessage response = client.PatchAsync($"/api/v3/admin/levels/id/{level.LevelId}", JsonContent.Create(payload)).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();
        Assert.Multiple(() =>
        {
            Assert.That(level.Title, Is.EqualTo("Updated"));
            Assert.That(level.GameVersion, Is.EqualTo(TokenGame.LittleBigPlanetPSP));
            Assert.That(level.UpdateDate, Is.Not.EqualTo(oldUpdate));
            Assert.That(level.UpdateDate, Is.EqualTo(context.Time.TimestampMilliseconds));
        });
    }
    
    [Test]
    public void CantUpdateMissingLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user, "Not updated");

        ApiEditLevelRequest payload = new()
        {
            Title = "Updated",
        };
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        HttpResponseMessage response = client.PatchAsync($"/api/v3/levels/id/{int.MaxValue}", JsonContent.Create(payload)).Result;
        Assert.That(response.StatusCode, Is.EqualTo(NotFound));
        
        context.Database.Refresh();
        Assert.Multiple(() =>
        {
            Assert.That(level.Title, Is.EqualTo("Not updated"));
        });
    }

    [Test]
    public void UpdatingLevelPersistsPublishDate()
    {
        using TestContext context = this.GetServer(false);
        GameUser author = context.CreateUser();

        context.Time.TimestampMilliseconds = 1;
        GameLevel level = context.CreateLevel(author);
        Assert.Multiple(() =>
        {
            Assert.That(level.PublishDate, Is.EqualTo(1));
            Assert.That(level.UpdateDate, Is.EqualTo(1));
        });

        GameLevel newLevel = (GameLevel)level.Clone();
        // When originating from a request, it wouldn't pass down the original PublishDate.
        // Replicate this here.
        newLevel.PublishDate = default;

        context.Time.TimestampMilliseconds = 2;
        context.Database.UpdateLevel(newLevel, author);
        Assert.Multiple(() =>
        {
            Assert.That(level.PublishDate, Is.EqualTo(1));
            Assert.That(level.UpdateDate, Is.EqualTo(2));
        });
    }
}