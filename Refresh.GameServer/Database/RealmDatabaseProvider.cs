using System.Diagnostics.CodeAnalysis;
using Realms;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.HttpServer.Database;

namespace Refresh.GameServer.Database;

public class RealmDatabaseProvider : IDatabaseProvider<RealmDatabaseContext>
{
    private RealmConfiguration _configuration = null!;

    [SuppressMessage("ReSharper", "InvertIf")]
    public void Initialize()
    {
        this._configuration = new RealmConfiguration(Path.Join(Environment.CurrentDirectory, "refreshGameServer.realm"))
        {
            SchemaVersion = 13,
            Schema = new[]
            {
                typeof(GameUser),
                typeof(GameLocation),
                typeof(UserPins),
                typeof(Token),
                typeof(GameLevel),
                typeof(GameComment),
            },
            MigrationCallback = (migration, oldVersion) =>
            {
                // Get the current unix timestamp for when we add timestamps to objects
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                
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
                    if (oldVersion < 4) newUser.Pins = new UserPins();

                    // In version 12, users were given IconHashes
                    if (oldVersion < 12) newUser.IconHash = "0";
                    
                    // In version 13, users were given PlanetsHashes
                    if (oldVersion < 13) newUser.PlanetsHash = "0";
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

                    // In version 11, timestamps were added to levels.
                    if (oldVersion < 11)
                    {
                        // Since we dont have a reference point for when the level was actually uploaded, default to now
                        newLevel.PublishDate = timestamp;
                        newLevel.UpdateDate = timestamp;
                    }
                }
            },
        };
    }

    private readonly ThreadLocal<Realm> _realmStorage = new(true);
    public RealmDatabaseContext GetContext() 
    {
        this._realmStorage.Value ??= Realm.GetInstance(this._configuration);
        
        return new RealmDatabaseContext(this._realmStorage.Value);
    }
    
    public void Dispose() 
    {
        foreach (Realm realmStorageValue in this._realmStorage.Values) 
        {
            realmStorageValue.Refresh();
            realmStorageValue.Dispose();
        }

        this._realmStorage.Dispose();
    }
}