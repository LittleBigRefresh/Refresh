using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;
using Refresh.Workers.State;

namespace RefreshTests.GameServer.Tests.Workers;

public class MigrationTests : GameServerTest
{
    [Test]
    public void MigrationJobWorks()
    {
        using TestContext context = this.GetServer();
        TestMigrationJob job = new();
        GameUser user = context.CreateUser();

        for (int i = 0; i < 100; i++)
        {
            context.CreateLevel(user);
        }

        IEnumerable<GameLevel> allLevels = context.Database.GetNewestLevels(100, 0, null, new LevelFilterSettings(TokenGame.Website)).Items;
        Assert.That(allLevels.All(l => l.Title == "Level"), Is.True);

        job.JobState = new MigrationJobState();
        job.ExecuteJob(context.GetWorkContext());
        
        allLevels = context.Database.GetNewestLevels(100, 0, null, new LevelFilterSettings(TokenGame.Website)).Items;
        Assert.That(allLevels.All(l => l.Title == "Level test"), Is.True);
    }
}