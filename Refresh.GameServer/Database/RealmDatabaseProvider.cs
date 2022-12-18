using Realms;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Levels;
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
            SchemaVersion = 10,
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
                
                // IQueryable<dynamic>? oldLevels = migration.OldRealm.DynamicApi.All("GameLevel");
                IQueryable<GameLevel>? newLevels = migration.NewRealm.All<GameLevel>();

                for (int i = 0; i < newLevels.Count(); i++)
                {
                    // dynamic oldLevel = oldLevels.ElementAt(i);
                    GameLevel newLevel = newLevels.ElementAt(i);
                    
                    // In version 10, GameLevels switched to int-based ids.
                    if (oldVersion < 10)
                    {
                        newLevel.LevelId = i + 1;
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