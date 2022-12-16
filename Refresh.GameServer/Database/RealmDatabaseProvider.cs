using Realms;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.UserData;
using Refresh.HttpServer.Database;

namespace Refresh.GameServer.Database;

public class RealmDatabaseProvider : IDatabaseProvider<RealmDatabaseContext>
{
    private RealmConfiguration _configuration = null!;

    public void Initialize()
    {
        this._configuration = new RealmConfiguration(Path.Join(Environment.CurrentDirectory, "refreshGameServer.realm"))
        {
            SchemaVersion = 8,
            Schema = new[]
            {
                typeof(GameUser),
                typeof(GameLocation),
                typeof(UserPins),
                typeof(Token),
                typeof(GameLevel),
            },
            MigrationCallback = (migration, oldVersion) =>
            {
                // IQueryable<dynamic>? oldUsers = migration.OldRealm.DynamicApi.All("GameUser");
                IQueryable<GameUser>? newUsers = migration.NewRealm.All<GameUser>();

                for (int i = 0; i < newUsers.Count(); i++)
                {
                    // dynamic oldUser = oldUsers.ElementAt(i);
                    GameUser newUser = newUsers.ElementAt(i);

                    if (oldVersion < 3)
                    {
                        newUser.Description = "";
                        newUser.Location = new GameLocation
                        {
                            X = 0,
                            Y = 0,
                        };
                    }
                    
                    //In version 4, GameLocation went from TopLevel -> Embedded, and UserPins was added
                    if (oldVersion < 4) {
                        newUser.Pins = new UserPins();
                    }
                }
            },
        };
    }

    public RealmDatabaseContext GetContext()
    {
        return new RealmDatabaseContext(Realm.GetInstance(this._configuration));
    }
}