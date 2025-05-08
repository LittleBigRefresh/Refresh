using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace RefreshTests.GameServer.Tests.Levels;

public class UploadTests : GameServerTest
{
    [Test]
    public void CanCreateLevelDirectly()
    {
        using TestContext context = this.GetServer(false);
        GameUser user = context.CreateUser();

        GameLevel level = new()
        {
            Title = "This is a level",
            Description = "incredible",
            Publisher = user,
        };
        
        context.Database.AddLevel(level);
        
        Assert.That(context.Database.GetLevelById(1), Is.Not.Null);
    }

    [Test]
    public void CantCreateLevelWithoutPublisher()
    {
        using TestContext context = this.GetServer(false);
        GameLevel level = new()
        {
            Title = "I AM AN ORPHAN!!!!",
            Publisher = null,
        };
        
        Assert.That(() => context.Database.AddLevel(level), Throws.InvalidOperationException);
    }

    [Test]
    public void CanUpdateLevel()
    {
        using TestContext context = this.GetServer(false);
        GameUser user = context.CreateUser();

        GameLevel level = new()
        {
            Title = "This is a level",
            Description = "incredible",
            Publisher = user,
        };
        
        context.Database.AddLevel(level);

        GameLevel levelUpdate = new()
        {
            LevelId = level.LevelId,
            Title = "This is a better level",
            Description = "incredible.",
            Publisher = user,
        };

        GameLevel? updatedLevel = context.Database.UpdateLevel(levelUpdate, user);
        Assert.Multiple(() =>
        {
            Assert.That(updatedLevel, Is.Not.Null);
            Assert.That(updatedLevel!.Description, Does.EndWith("."));
            Assert.That(updatedLevel.Title, Is.EqualTo("This is a better level"));
        });
    }

    [Test]
    public void CantUpdateOtherUsersLevels()
    {
        using TestContext context = this.GetServer(false);
        
        GameUser author = context.CreateUser();
        GameUser baddie = context.CreateUser();
        
        GameLevel level = context.CreateLevel(author);

        GameLevel levelUpdate = new()
        {
            LevelId = level.LevelId,
            RootResource = "Malware",
            Publisher = baddie,
        };

        GameLevel? updatedLevel = context.Database.UpdateLevel(levelUpdate, baddie);
        Assert.Multiple(() =>
        {
            Assert.That(updatedLevel, Is.Null);
            Assert.That(level.RootResource, Is.Not.EqualTo("Malware"));
            Assert.That(level.Publisher, Is.Not.EqualTo(baddie));
            Assert.That(level.Publisher, Is.EqualTo(author));
        });
    }

    [Test]
    public void CantUpdateNonExistentLevels()
    {
        using TestContext context = this.GetServer(false);
        
        GameUser author = context.CreateUser();
        GameLevel levelUpdate = new()
        {
            LevelId = 69696969,
            Publisher = author,
        };

        GameLevel? updatedLevel = context.Database.UpdateLevel(levelUpdate, author);
        
        Assert.That(updatedLevel, Is.Null);
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
}