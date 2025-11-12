using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Moderation;
using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Moderation;

public class ModerationActionTests : GameServerTest
{
    [Test]
    public void CreateUserModerationAction()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameUser mod = context.CreateUser();
        GameUser unrelated = context.CreateUser();

        // Create action
        ModerationAction action = context.Database.CreateModerationAction(user, ModerationActionType.UserModification, mod, "Cringe description");

        // Retrieve all actions
        DatabaseList<ModerationAction> list = context.Database.GetModerationActions(0, 10);
        Assert.That(list.TotalItems, Is.EqualTo(1));
        Assert.That(list.Items.Count(), Is.EqualTo(1));
        Assert.That(list.Items.First().ActionId, Is.EqualTo(action.ActionId));

        // Retrieve actions for moderator
        list = context.Database.GetModerationActionsByActor(mod, 0, 10);
        Assert.That(list.TotalItems, Is.EqualTo(1));
        Assert.That(list.Items.Count(), Is.EqualTo(1));
        Assert.That(list.Items.First().ActionId, Is.EqualTo(action.ActionId));

        // Retrieve actions for user
        list = context.Database.GetModerationActionsForInvolvedUser(user, 0, 10);
        Assert.That(list.TotalItems, Is.EqualTo(1));
        Assert.That(list.Items.Count(), Is.EqualTo(1));
        Assert.That(list.Items.First().ActionId, Is.EqualTo(action.ActionId));

        // Retrieve actions for user profile
        list = context.Database.GetModerationActionsForObject(user.UserId.ToString(), ModerationObjectType.User, 0, 10);
        Assert.That(list.TotalItems, Is.EqualTo(1));
        Assert.That(list.Items.Count(), Is.EqualTo(1));
        Assert.That(list.Items.First().ActionId, Is.EqualTo(action.ActionId));

        // Hide from unrelated user
        list = context.Database.GetModerationActionsByActor(unrelated, 0, 10);
        Assert.That(list.TotalItems, Is.EqualTo(0));
        Assert.That(list.Items.Count(), Is.EqualTo(0));

        list = context.Database.GetModerationActionsForInvolvedUser(unrelated, 0, 10);
        Assert.That(list.TotalItems, Is.EqualTo(0));
        Assert.That(list.Items.Count(), Is.EqualTo(0));
    }

    [Test]
    public void CreateLevelModerationAction()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameUser mod = context.CreateUser();
        GameUser unrelated = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        // Create action
        ModerationAction action = context.Database.CreateModerationAction(level, ModerationActionType.LevelModification, mod, "Cringe icon");

        // Retrieve all actions
        DatabaseList<ModerationAction> list = context.Database.GetModerationActions(0, 10);
        Assert.That(list.TotalItems, Is.EqualTo(1));
        Assert.That(list.Items.Count(), Is.EqualTo(1));
        Assert.That(list.Items.First().ActionId, Is.EqualTo(action.ActionId));

        // Retrieve actions for moderator
        list = context.Database.GetModerationActionsByActor(mod, 0, 10);
        Assert.That(list.TotalItems, Is.EqualTo(1));
        Assert.That(list.Items.Count(), Is.EqualTo(1));
        Assert.That(list.Items.First().ActionId, Is.EqualTo(action.ActionId));

        // Retrieve actions for user
        list = context.Database.GetModerationActionsForInvolvedUser(user, 0, 10);
        Assert.That(list.TotalItems, Is.EqualTo(1));
        Assert.That(list.Items.Count(), Is.EqualTo(1));
        Assert.That(list.Items.First().ActionId, Is.EqualTo(action.ActionId));

        // Retrieve actions for level
        list = context.Database.GetModerationActionsForObject(level.LevelId.ToString(), ModerationObjectType.Level, 0, 10);
        Assert.That(list.TotalItems, Is.EqualTo(1));
        Assert.That(list.Items.Count(), Is.EqualTo(1));
        Assert.That(list.Items.First().ActionId, Is.EqualTo(action.ActionId));

        // Hide from unrelated user
        list = context.Database.GetModerationActionsByActor(unrelated, 0, 10);
        Assert.That(list.TotalItems, Is.EqualTo(0));
        Assert.That(list.Items.Count(), Is.EqualTo(0));

        list = context.Database.GetModerationActionsForInvolvedUser(unrelated, 0, 10);
        Assert.That(list.TotalItems, Is.EqualTo(0));
        Assert.That(list.Items.Count(), Is.EqualTo(0));
    }
}