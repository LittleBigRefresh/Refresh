using System.Globalization;
using Refresh.Common.Constants;
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
            Assert.That(level.OriginalPublisher, Is.EqualTo(SystemUsers.UnknownUserName));
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
    
    
    [Test]
    [TestCase("?date.05-04-15 \n gamer\n")]
    [TestCase("?date:05-04-15\n")]
    [TestCase("?date.05-04-15 ")]
    [TestCase("?date.05-04-15\r\n")]
    public void OriginalUploadDateDetectedInDescription(string prefix)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user, "All The Boxes (reupload)", $"{prefix}One big level. Full of boxes.");
        
        DateTimeOffset.TryParseExact("05-04-15", "dd-MM-yy", null, DateTimeStyles.None, out DateTimeOffset expectedDate);
            
        Assert.That(level.PublishDate, Is.EqualTo(expectedDate));
    }
    
    
    [Test]
    [TestCase("?op:gamer1 ?date:05-05-20", "gamer1", "05-05-20")]
    [TestCase("?date.15-07-25 ?op.player2", "player2", "15-07-25")]
    [TestCase("?op:ReallyLongUserName\n?date:01-01-25", "ReallyLongUserName", "01-01-25")]
    [TestCase("?extra:test ?date:10-12-10 \n\r ?op:admin", "admin", "10-12-10")]
    public void MultipleAttributesDetectedInDescription(string attributeString, string expectedPublisher,
        string expectedDateString)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user, "All The Boxes (reupload)", $"{attributeString} One big level. Full of boxes.");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(level.IsReUpload, Is.True);
            Assert.That(level.OriginalPublisher, Is.EqualTo(expectedPublisher));

            DateTimeOffset.TryParseExact(expectedDateString, "dd-MM-yy", null, DateTimeStyles.None, out DateTimeOffset expectedDate);
            
            Assert.That(level.PublishDate, Is.EqualTo(expectedDate));
        }
    }
}
