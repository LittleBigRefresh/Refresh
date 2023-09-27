using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

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
        this._db.Dispose();
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
    public void GetByLegacyId()
    {
        Assert.Multiple(() =>
        {
            Assert.That(this._db.GetUserByLegacyId(null), Is.Null);
            Assert.That(this._db.GetUserByLegacyId(this._user.UserId.Timestamp), Is.EqualTo(this._user));
        });
    }

    [Test]
    public void GetTotalCount()
    {
        Assert.That(this._db.GetTotalUserCount(), Is.EqualTo(1));
    }
}