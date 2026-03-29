using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Request;
using Refresh.Database.Models.Authentication;

namespace RefreshTests.GameServer.Tests.Levels;

public class UploadTests : GameServerTest
{
    [Test]
    public void CanCreateLevelDirectly()
    {
        using TestContext context = this.GetServer(false);
        GameUser user = context.CreateUser();

        GameLevelRequest level = new()
        {
            Title = "This is a level",
            Description = "incredible",
            RootResource = "totally a level",
        };
        
        context.Database.AddLevel(level, TokenGame.LittleBigPlanet1, user);
        Assert.That(context.Database.GetLevelById(1), Is.Not.Null);
    }

    [Test]
    public void CanCreateMultipleLevelsWhileRefreshing()
    {
        using TestContext context = this.GetServer(false);
        GameUser user = context.CreateUser();

        context.Database.Refresh();
        GameLevel level1 = context.CreateLevel(user);
        context.Database.Refresh();
        GameLevel level2 = context.CreateLevel(user);
        context.Database.Refresh();
        GameLevel level3 = context.CreateLevel(user);
        context.Database.Refresh();
    }

    [Test]
    public void CanUpdateLevel()
    {
        using TestContext context = this.GetServer(false);
        GameUser user = context.CreateUser();

        GameLevelRequest levelRequest = new()
        {
            Title = "This is a level",
            Description = "incredible",
            RootResource = "totally a level",
        };
        
        GameLevel level = context.Database.AddLevel(levelRequest, TokenGame.LittleBigPlanet1, user);

        GameLevelRequest levelUpdate = new()
        {
            LevelId = level.LevelId,
            Title = "This is a better level",
            Description = "incredible.",
            RootResource = "also a level",
        };

        GameLevel? updatedLevel = context.Database.UpdateLevel(levelUpdate, level);
        Assert.Multiple(() =>
        {
            Assert.That(updatedLevel, Is.Not.Null);
            Assert.That(updatedLevel!.Description, Does.EndWith("."));
            Assert.That(updatedLevel.Title, Is.EqualTo("This is a better level"));
        });
    }

    [Test]
    public void CanDeleteLevel()
    {
        using TestContext context = this.GetServer(false);

        GameLevel level = context.CreateLevel(context.CreateUser());
        int id = level.LevelId;
        
        Assert.That(context.Database.GetLevelById(id), Is.Not.Null);
        context.Database.DeleteLevel(level);
        Assert.That(context.Database.GetLevelById(id), Is.Null);
    }

    [Test]
    public void LevelRootUpdateChangesUpdateDate()
    {
        using TestContext context = this.GetServer(false);
        GameUser author = context.CreateUser();

        context.Time.TimestampMilliseconds = 1;
        GameLevel level = context.CreateLevel(author);
        Assert.Multiple(() =>
        {
            Assert.That(level.PublishDate.ToUnixTimeMilliseconds(), Is.EqualTo(1));
            Assert.That(level.UpdateDate.ToUnixTimeMilliseconds(), Is.EqualTo(1));
        });
        
        GameLevelRequest newLevel = new()
        {
            RootResource = "g12345",
        };

        context.Time.TimestampMilliseconds = 2;
        context.Database.UpdateLevel(newLevel, level);
        Assert.Multiple(() =>
        {
            Assert.That(level.PublishDate.ToUnixTimeMilliseconds(), Is.EqualTo(1));
            Assert.That(level.UpdateDate.ToUnixTimeMilliseconds(), Is.EqualTo(2));
        });
    }
    
    [Test]
    public void LevelUpdateDoesNotChangeUpdateDateWhenRootUnchanged()
    {
        using TestContext context = this.GetServer(false);
        GameUser author = context.CreateUser();

        context.Time.TimestampMilliseconds = 1;
        GameLevel level = context.CreateLevel(author);
        Assert.Multiple(() =>
        {
            Assert.That(level.PublishDate.ToUnixTimeMilliseconds(), Is.EqualTo(1));
            Assert.That(level.UpdateDate.ToUnixTimeMilliseconds(), Is.EqualTo(1));
        });
        
        GameLevelRequest newLevel = new()
        {
            Description = "description update",
            RootResource = level.RootResource,
        };

        context.Time.TimestampMilliseconds = 2;
        context.Database.UpdateLevel(newLevel, level);
        Assert.Multiple(() =>
        {
            Assert.That(level.PublishDate.ToUnixTimeMilliseconds(), Is.EqualTo(1));
            Assert.That(level.UpdateDate.ToUnixTimeMilliseconds(), Is.EqualTo(1));
        });
    }
}