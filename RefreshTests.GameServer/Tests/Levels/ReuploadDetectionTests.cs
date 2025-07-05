using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Levels;

public class ReuploadDetectionTests : GameServerTest
{
    [Test]
    public void LevelPublishesNormally()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user, "All The Boxes", "One big level. Full of boxes.");
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(level.IsReUpload, Is.False);
            Assert.That(level.OriginalPublisher, Is.Null);
        }
    }
    
    [Test]
    [TestCase("[reupload]")]
    [TestCase("[REUPLOAD]")]
    [TestCase("(REUPLOAD)")]
    [TestCase("(ReUpload)")]
    [TestCase("(reupload)")]
    public void ReuploadDetectedInTitle(string suffix)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user, "All The Boxes " + suffix, "One big level. Full of boxes.");
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(level.IsReUpload, Is.True);
            Assert.That(level.OriginalPublisher, Is.Null);
        }
    }
    
    [Test]
    [TestCase("?op.gamer1\n")]
    [TestCase("?op:gamer1\n")]
    [TestCase("?op.gamer1 ")]
    [TestCase("?op.gamer1\r\n")]
    public void OriginalPublisherDetectedInDescription(string prefix)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user, "All The Boxes (reupload)", $"{prefix}One big level. Full of boxes.");
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(level.IsReUpload, Is.True);
            Assert.That(level.OriginalPublisher, Is.EqualTo("gamer1"));
        }
    }
}