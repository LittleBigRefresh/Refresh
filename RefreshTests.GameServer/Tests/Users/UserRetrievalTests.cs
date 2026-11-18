using Refresh.Database;
using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Users;

public class UserRetrievalTests : GameServerTest
{
    private TestContext _context = null!;
    private GameUser _user = null!;
    private GameDatabaseContext _db = null!;
    
    [OneTimeSetUp]
    public void SetUp()
    {
        this._context = this.GetServer(false);
        this._user = this._context.CreateUser("username");
        this._db = this._context.Database;
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        this._context.Dispose();
    }

    [Test]
    public void GetByObjectId()
    {
        Assert.Multiple(() =>
        {
            Assert.That(this._db.GetUserByObjectId(null), Is.Null);
            Assert.That(this._db.GetUserByObjectId(this._user.UserId), Is.EqualTo(this._user));
        });
    }

    [Test]
    public void GetByUsername()
    {
        Assert.Multiple(() =>
        {
            Assert.That(this._db.GetUserByUsername(null), Is.Null);
            Assert.That(this._db.GetUserByUsername("username"), Is.EqualTo(this._user));
        });
    }

    [Test]
    public void GetByUuid()
    {
        Assert.Multiple(() =>
        {
            Assert.That(this._db.GetUserByUuid(null), Is.Null);
            Assert.That(this._db.GetUserByUuid("z"), Is.Null); // invalid object id
            Assert.That(this._db.GetUserByUuid(this._user.UserId.ToString()), Is.EqualTo(this._user));
        });
    }


    [Test]
    public void GetTotalCount()
    {
        Assert.That(this._db.GetTotalUserCount(), Is.EqualTo(1));
    }

    [Test]
    public void GetsUserByVariousUsernameCasings()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.Database.CreateUser("TheName", "legit@real.lol");

        GameUser? exactCasingUser = context.Database.GetUserByUsername("TheName", true); // lookup by exact username
        GameUser? WrongCasingAllowedUser = context.Database.GetUserByUsername("tHEnAMe", false); // lookup by wrong casing while allowing it
        GameUser? WrongCasingDisallowedUser = context.Database.GetUserByUsername("tHEnAMe", true); // lookup by wrong casing while not allowing it
        GameUser? WrongCasingDefaultUser = context.Database.GetUserByUsername("tHEnAMe"); // assert that default value of caseSensitive is true

        Assert.Multiple(() =>
        {
            // We found a user by exact casing
            Assert.That(exactCasingUser, Is.Not.Null);
            Assert.That(exactCasingUser!.UserId, Is.EqualTo(user.UserId));

            // Found user by mismatching casing because we've allowed it
            Assert.That(WrongCasingAllowedUser, Is.Not.Null);
            Assert.That(WrongCasingAllowedUser!.UserId, Is.EqualTo(user.UserId));

            // Couldn't find user with mismatching casing if we've expected a user by exact casing
            Assert.That(WrongCasingDisallowedUser, Is.Null);

            // Couldn't find user with mismatching casing because we look up case sensitively by default
            Assert.That(WrongCasingDefaultUser, Is.Null);
        });
    }

    [Test]
    public void DoesNotLookUpByUsernameIfUuidDoesNotResolveToUser()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.Database.CreateUser("TheName", "legit@real.lol");

        GameUser? notFoundUuidUser = context.Database.GetUserByUuidOrUsername("eee", "TheName", true);
        GameUser? notGivenUuidUser = context.Database.GetUserByUuidOrUsername(null, "TheName", true);

        Assert.Multiple(() =>
        {
            // Given UUID doesn't resolve to a user, but username does -> username is ignored and null is returned
            Assert.That(notFoundUuidUser, Is.Null);

            // No UUID given -> attempt to look up by username
            Assert.That(notGivenUuidUser, Is.Not.Null);
            Assert.That(notGivenUuidUser!.UserId, Is.EqualTo(user.UserId));
        });
    }
}