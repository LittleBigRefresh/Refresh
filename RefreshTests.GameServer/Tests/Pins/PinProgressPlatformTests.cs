using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Pins;
using Refresh.Database.Models.Relations;
using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Pins;

public class PinProgressPlatformTests : GameServerTest
{
    [Test]
    public void PlatformedPinsSeperatedByPlatformTest()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        // ROUND 1 - Create a pin which has a non-website platform (not universal)
        context.Database.IncrementUserPinProgress((long)ServerPins.TopXOfAnyStoryLevelWithOver50Scores, 420, user, false, TokenPlatform.PS3);

        // Pin should appear when searching for this specific game type and platform
        DatabaseList<PinProgressRelation> relations = context.Database.GetPinProgressesByUser(user, false, TokenPlatform.PS3, 0, 300);
        Assert.That(relations.Items.Count(), Is.EqualTo(1));
        Assert.That(relations.Items.First().PinId, Is.EqualTo((long)ServerPins.TopXOfAnyStoryLevelWithOver50Scores));

        // Pin should not appear if searched game type is wrong
        relations = context.Database.GetPinProgressesByUser(user, true, TokenPlatform.PS3, 0, 300);
        Assert.That(relations.Items.Count(), Is.Zero);

        // Pin should not appear if searched platform is wrong
        relations = context.Database.GetPinProgressesByUser(user, false, TokenPlatform.Vita, 0, 300);
        Assert.That(relations.Items.Count(), Is.Zero);

        // ROUND 2 - Update the pin to lowest
        context.Database.UpdateUserPinProgressToLowest((long)ServerPins.TopXOfAnyStoryLevelWithOver50Scores, 210, user, false, TokenPlatform.PS3);

        // Pin should appear when searching for this specific game type and platform
        relations = context.Database.GetPinProgressesByUser(user, false, TokenPlatform.PS3, 0, 300);
        Assert.That(relations.Items.Count(), Is.EqualTo(1));
        Assert.That(relations.Items.First().PinId, Is.EqualTo((long)ServerPins.TopXOfAnyStoryLevelWithOver50Scores));

        // Pin should not appear if searched game type is wrong
        relations = context.Database.GetPinProgressesByUser(user, true, TokenPlatform.PS3, 0, 300);
        Assert.That(relations.Items.Count(), Is.Zero);

        // Pin should not appear if searched platform is wrong
        relations = context.Database.GetPinProgressesByUser(user, false, TokenPlatform.Vita, 0, 300);
        Assert.That(relations.Items.Count(), Is.Zero);
    }

    [Test]
    public void UniversalPinsAppearOnAllPlatformsTest()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        // ROUND 1 - Create a pin which is "universal" (platform is Website)
        context.Database.IncrementUserPinProgress((long)ServerPins.SignIntoWebsite, 420, user, false, TokenPlatform.Website);

        // Pin should appear when searching for any specific game type and platform
        DatabaseList<PinProgressRelation> relations = context.Database.GetPinProgressesByUser(user, false, TokenPlatform.PS3, 0, 300);
        Assert.That(relations.Items.Count(), Is.EqualTo(1));
        Assert.That(relations.Items.First().PinId, Is.EqualTo((long)ServerPins.SignIntoWebsite));

        // Pin should appear if searched game type is wrong
        relations = context.Database.GetPinProgressesByUser(user, true, TokenPlatform.PS3, 0, 300);
        Assert.That(relations.Items.Count(), Is.EqualTo(1));
        Assert.That(relations.Items.First().PinId, Is.EqualTo((long)ServerPins.SignIntoWebsite));

        // Pin should appear if searched platform is wrong
        relations = context.Database.GetPinProgressesByUser(user, false, TokenPlatform.Vita, 0, 300);
        Assert.That(relations.Items.Count(), Is.EqualTo(1));
        Assert.That(relations.Items.First().PinId, Is.EqualTo((long)ServerPins.SignIntoWebsite));

        // Pin should appear if both are wrong
        relations = context.Database.GetPinProgressesByUser(user, true, TokenPlatform.RPCS3, 0, 300);
        Assert.That(relations.Items.Count(), Is.EqualTo(1));
        Assert.That(relations.Items.First().PinId, Is.EqualTo((long)ServerPins.SignIntoWebsite));

        // ROUND 2 - Update the pin to lowest (through the game)
        context.Database.UpdateUserPinProgressToLowest((long)ServerPins.SignIntoWebsite, 210, user, false, TokenPlatform.PS3);

        // Pin should appear if searched game type is wrong
        relations = context.Database.GetPinProgressesByUser(user, true, TokenPlatform.PS3, 0, 300);
        Assert.That(relations.Items.Count(), Is.EqualTo(1));
        Assert.That(relations.Items.First().PinId, Is.EqualTo((long)ServerPins.SignIntoWebsite));

        // Pin should appear if searched platform is wrong
        relations = context.Database.GetPinProgressesByUser(user, false, TokenPlatform.Vita, 0, 300);
        Assert.That(relations.Items.Count(), Is.EqualTo(1));
        Assert.That(relations.Items.First().PinId, Is.EqualTo((long)ServerPins.SignIntoWebsite));

        // Pin should appear if both are wrong
        relations = context.Database.GetPinProgressesByUser(user, true, TokenPlatform.RPCS3, 0, 300);
        Assert.That(relations.Items.Count(), Is.EqualTo(1));
        Assert.That(relations.Items.First().PinId, Is.EqualTo((long)ServerPins.SignIntoWebsite));
    }
}