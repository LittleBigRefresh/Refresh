using Realms;
using Realms.Schema;
using Refresh.GameServer.Database.Types;
using Refresh.HttpServer.Database;

namespace Refresh.GameServer.Database;

public class RealmDatabaseProvider : IDatabaseProvider<RealmDatabaseContext>
{
    private RealmConfiguration _configuration = null!;

    public void Initialize()
    {
        this._configuration = new RealmConfiguration(Path.Join(Environment.CurrentDirectory, "refreshGameServer.realm"))
        {
            Schema = new[]
            {
                typeof(GameUser),
            },
        };
    }

    public RealmDatabaseContext GetContext()
    {
        return new RealmDatabaseContext(Realm.GetInstance(this._configuration));
    }
}