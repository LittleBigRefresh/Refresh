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

    // These ones don't use the shared contexts above

    [Test]
    [TestCase("SuperCasing", "supercasing")]
    [TestCase("supercasing", "SuperCasing")] // swapped, so they'll hopefully be in different positions in the table
    public void GetByExactUsernameEvenIfCaseInsensitive(string firstName, string secondName)
    {
        using TestContext context = this.GetServer();
        GameUser firstUser = context.Database.CreateUser(firstName, "legit@real.lol");
        GameUser secondUser = context.Database.CreateUser(secondName, "cool@real.lol");

        GameUser? firstFetch = context.Database.GetUserByUsername(firstName, false);
        GameUser? secondFetch = context.Database.GetUserByUsername(secondName, false);

        Assert.Multiple(() =>
        {
            // Won't get the user with the wrong name on either one of those
            Assert.That(firstFetch, Is.Not.Null);
            Assert.That(firstFetch!.Username, Is.EqualTo(firstName));
            Assert.That(firstFetch!.UserId, Is.EqualTo(firstUser.UserId));

            Assert.That(secondFetch, Is.Not.Null);
            Assert.That(secondFetch!.Username, Is.EqualTo(secondName));
            Assert.That(secondFetch!.UserId, Is.EqualTo(secondUser.UserId));
        });
    }
}