using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Database.Types;
using Refresh.HttpServer.Database;

namespace Refresh.GameServer.Database;

public class RealmDatabaseContext : IDatabaseContext
{
    private readonly Realm _realm;

    public RealmDatabaseContext(Realm realm)
    {
        this._realm = realm;
    }

    public void Dispose()
    {
        this._realm.Refresh();
        this._realm.Dispose();
    }

    public GameUser CreateUser(string username)
    {
        GameUser user = new()
        {
            Username = username,
        };

        this._realm.Write(() =>
        {
            this._realm.Add(user);
        });
        return user;
    }

    public GameUser? GetUser(ObjectId id) => this._realm.Find<GameUser>(id);
    public GameUser? GetUser(string username) => this._realm.All<GameUser>().FirstOrDefault(u => u.Username == username);
}