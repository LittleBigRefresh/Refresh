using System.Diagnostics.CodeAnalysis;
using Realms;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Bunkum.RealmDatabase;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.Report;

namespace Refresh.GameServer.Database;

public class GameDatabaseProvider : RealmDatabaseProvider<GameDatabaseContext>
{
    protected override ulong SchemaVersion => 35;

    protected override string Filename => "refreshGameServer.realm";
    
    protected override List<Type> SchemaTypes { get; } = new()
    {
        typeof(GameUser),
        typeof(GameLocation),
        typeof(UserPins),
        typeof(Token),
        typeof(GameLevel),
        typeof(GameComment),
        typeof(FavouriteLevelRelation),
        typeof(QueueLevelRelation),
        typeof(FavouriteUserRelation),
        typeof(Event),
        //grief report items
        typeof(GriefReport),
        typeof(InfoBubble),
        typeof(Marqee),
        typeof(Player),
        typeof(Rect),
        typeof(ScreenElements),
        typeof(ScreenRect),
        typeof(Slot),
    };
    
    protected override void Migrate(Migration migration, ulong oldVersion)
    {
        // Get the current unix timestamp for when we add timestamps to objects
        long timestampMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // DO NOT USE FOR NEW MIGRATIONS! LBP almost never actually uses seconds for timestamps.
        // This is from a mistake made early in development where this was not understood by me.
        // Unless you are certain second timestamps are used, use the millisecond timestamps set above.
        long timestampSeconds = timestampMilliseconds / 1000;

        // IQueryable<dynamic>? oldUsers = migration.OldRealm.DynamicApi.All("GameUser");
        IQueryable<GameUser>? newUsers = migration.NewRealm.All<GameUser>();

        for (int i = 0; i < newUsers.Count(); i++)
        {
            // dynamic oldUser = oldUsers.ElementAt(i);
            GameUser newUser = newUsers.ElementAt(i);

            if (oldVersion < 3)
            {
                newUser.Description = "";
                newUser.Location = new GameLocation { X = 0, Y = 0, };
            }

            //In version 4, GameLocation went from TopLevel -> Embedded, and UserPins was added
            if (oldVersion < 4) newUser.Pins = new UserPins();

            // In version 12, users were given IconHashes
            if (oldVersion < 12) newUser.IconHash = "0";

            // In version 13, users were given PlanetsHashes
            if (oldVersion < 13) newUser.PlanetsHash = "0";

            // In version 23, users were given bcrypt passwords
            if (oldVersion < 23) newUser.PasswordBcrypt = null;

            // In version 26, users were given join dates
            if (oldVersion < 26) newUser.JoinDate = 0;
        }

        IQueryable<dynamic>? oldLevels = migration.OldRealm.DynamicApi.All("GameLevel");
        IQueryable<GameLevel>? newLevels = migration.NewRealm.All<GameLevel>();

        for (int i = 0; i < newLevels.Count(); i++)
        {
            dynamic oldLevel = oldLevels.ElementAt(i);
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
                newLevel.PublishDate = timestampSeconds;
                newLevel.UpdateDate = timestampSeconds;
            }

            // In version 14, level timestamps were fixed
            if (oldVersion < 14)
            {
                newLevel.PublishDate = oldLevel.PublishDate * 1000;
                newLevel.UpdateDate = oldLevel.UpdateDate * 1000;
            }
        }

        // In version 22, tokens added expiry and types so just wipe them all
        if (oldVersion < 22) migration.NewRealm.RemoveAll<Token>();
                
        // In version 35, tokens added platforms and games
        if (oldVersion < 35) migration.NewRealm.RemoveAll<Token>();

        // IQueryable<dynamic>? oldEvents = migration.OldRealm.DynamicApi.All("Event");
        IQueryable<Event>? newEvents = migration.NewRealm.All<Event>();

        for (int i = 0; i < newEvents.Count(); i++)
        {
            // dynamic oldEvent = oldEvents.ElementAt(i);
            Event newEvent = newEvents.ElementAt(i);

            // In version 30, events were given timestamps
            if (oldVersion < 30) newEvent.Timestamp = timestampSeconds;

            // Fixes events with broken timestamps
            if (oldVersion < 32 && newEvent.Timestamp == 0) newEvent.Timestamp = timestampSeconds;

            // Converts events to use millisecond timestamps
            if (oldVersion < 33 && newEvent.Timestamp < 1000000000000) newEvent.Timestamp *= 1000;
        }
    }
}