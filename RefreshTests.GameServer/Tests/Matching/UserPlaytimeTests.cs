using Refresh.Core.Configuration;
using Refresh.Core.Services;
using Refresh.Core.Types.Data;
using Refresh.Core.Types.Matching;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Metrics;
using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Matching;

public class UserPlaytimeTests : GameServerTest
{
    private DataContext GetDataContext(GameDatabaseContext database, Token token, MatchService match)
    {
        return new DataContext
        {
            Database = database,
            Logger = Logger,
            DataStore = null!, //this isn't accessed by matching
            Match = match,
            GuidChecker = null!,
            Token = token,
        };
    }

    [Test]
    public void IncrementsPlaytimeWhenNoLoginInbetween()
    {
        using TestContext context = this.GetServer(false);
        GameUser user = context.CreateUser();
        Token token = context.CreateToken(user, TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);

        GameServerConfig config = new();
        MatchService match = new(Logger);
        match.Initialize();

        SerializedRoomData roomData = new()
        {
            NatType = new List<NatType>
            {
                NatType.Open,
            },
        };

        // Ensure that the first room update will not add these first 10 ms to the playtime, but last update timestamp will be set.
        context.Time.TimestampMilliseconds += 10;
        match.ExecuteMethod("CreateRoom", roomData, this.GetDataContext(context.Database, token, match), config);
        context.Database.Refresh();

        UserGameMetric metric = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        Assert.That(metric.LastLoginAt, Is.EqualTo(DateTimeOffset.MinValue));
        Assert.That(metric.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(10));
        Assert.That(metric.TotalPlayTimeMinutes, Is.Zero);

        // Send another room update, and ensure that this time, the playtime was incremented by the difference between the
        // previous and current timestamps.
        // Also, ensure the total playtime stat was incremented aswell.
        context.Time.TimestampMilliseconds += 20;
        match.ExecuteMethod("UpdateMyPlayerData", roomData, this.GetDataContext(context.Database, token, match), config);
        context.Database.Refresh();

        metric = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        Assert.That(metric.LastLoginAt, Is.EqualTo(DateTimeOffset.MinValue));
        Assert.That(metric.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(30));
        Assert.That(metric.TotalPlayTimeMinutes, Is.EqualTo(10));

        GameUser? userUpdated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(userUpdated?.Statistics, Is.Not.Null);
        Assert.That(userUpdated!.Statistics!.TotalPlayTimeMinutes, Is.EqualTo(10));

        // Now let it update the login date, and ensure the login date has updated,
        // but the room update date and the playtime have not.
        context.Time.TimestampMilliseconds += 30;
        context.Database.UpdateLoginDateOnUserMetric(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        context.Database.Refresh();

        metric = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        Assert.That(metric.LastLoginAt.ToUnixTimeMilliseconds(), Is.EqualTo(60)); // is now above 0
        Assert.That(metric.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(30)); // still the same as before
        Assert.That(metric.TotalPlayTimeMinutes, Is.EqualTo(10)); // still the same as before

        userUpdated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(userUpdated?.Statistics, Is.Not.Null);
        Assert.That(userUpdated!.Statistics!.TotalPlayTimeMinutes, Is.EqualTo(10)); // still the same as before

        // Ensure the next room update updates the last update timestamp, but not the playtime.
        context.Time.TimestampMilliseconds += 40;
        match.ExecuteMethod("UpdateMyPlayerData", roomData, this.GetDataContext(context.Database, token, match), config);
        context.Database.Refresh();

        // Ensure both the total stat and the metric for this game/paltform are now above 0
        metric = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        Assert.That(metric.LastLoginAt.ToUnixTimeMilliseconds(), Is.EqualTo(60)); // The same as right now
        Assert.That(metric.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(100)); // timestamp has updated
        Assert.That(metric.TotalPlayTimeMinutes, Is.EqualTo(50)); // new time difference was added

        userUpdated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(userUpdated?.Statistics, Is.Not.Null);
        Assert.That(userUpdated!.Statistics!.TotalPlayTimeMinutes, Is.EqualTo(50)); // was also updated
    }

    [Test]
    public void IncrementsPlaytimeAcrossGameAndPlatform()
    {
        using TestContext context = this.GetServer(false);
        GameUser user = context.CreateUser();
        Token token1 = context.CreateToken(user, TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        // same as first but different platform
        Token token2 = context.CreateToken(user, TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3);
        // same as first but different game
        Token token3 = context.CreateToken(user, TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        // completely different game and plaftorm
        Token token4 = context.CreateToken(user, TokenType.Game, TokenGame.LittleBigPlanetVita, TokenPlatform.Vita);

        GameServerConfig config = new();
        MatchService match = new(Logger);
        match.Initialize();

        SerializedRoomData roomData = new()
        {
            NatType = new List<NatType>
            {
                NatType.Open,
            },
        };

        // -- Set the times --
        // LBP1 PS3
        context.Time.TimestampMilliseconds += 5;
        match.ExecuteMethod("CreateRoom", roomData, this.GetDataContext(context.Database, token1, match), config);
        context.Time.TimestampMilliseconds += 10;
        match.ExecuteMethod("UpdateMyPlayerData", roomData, this.GetDataContext(context.Database, token1, match), config);
        context.Database.Refresh();

        // LBP1 RPCS3
        match.ExecuteMethod("CreateRoom", roomData, this.GetDataContext(context.Database, token2, match), config);
        context.Time.TimestampMilliseconds += 20;
        match.ExecuteMethod("UpdateMyPlayerData", roomData, this.GetDataContext(context.Database, token2, match), config);
        context.Database.Refresh();

        // LBP2 PS3
        match.ExecuteMethod("CreateRoom", roomData, this.GetDataContext(context.Database, token3, match), config);
        context.Time.TimestampMilliseconds += 60;
        match.ExecuteMethod("UpdateMyPlayerData", roomData, this.GetDataContext(context.Database, token3, match), config);
        context.Database.Refresh();

        // LBPVita PSVita
        match.ExecuteMethod("CreateRoom", roomData, this.GetDataContext(context.Database, token4, match), config);
        context.Time.TimestampMilliseconds += 50;
        match.ExecuteMethod("UpdateMyPlayerData", roomData, this.GetDataContext(context.Database, token4, match), config);
        context.Database.Refresh();

        // -- Assertions --
        // LBP1 PS3
        UserGameMetric metric1 = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        Assert.That(metric1.TotalPlayTimeMinutes, Is.EqualTo(10));
        Assert.That(metric1.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(15));
        
        // LBP1 RPCS3
        UserGameMetric metric2 = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3);
        Assert.That(metric1.TotalPlayTimeMinutes, Is.EqualTo(20));
        Assert.That(metric1.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(35));
        
        // LBP2 PS3
        UserGameMetric metric3 = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        Assert.That(metric1.TotalPlayTimeMinutes, Is.EqualTo(60));
        Assert.That(metric1.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(95));
        
        // LBPVita PSVita
        UserGameMetric metric4 = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanetVita, TokenPlatform.Vita);
        Assert.That(metric1.TotalPlayTimeMinutes, Is.EqualTo(50));
        Assert.That(metric1.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(145));

        // Ensure the total playtime stat is actually a sum of all metric playtimes
        GameUser? userUpdated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(userUpdated?.Statistics, Is.Not.Null);
        Assert.That(userUpdated!.Statistics!.TotalPlayTimeMinutes, Is.EqualTo(140));
    }

    [Test]
    public void SetsLoginDateAcrossGameAndPlatform()
    {
        using TestContext context = this.GetServer(false);
        GameUser user = context.CreateUser();
        Token token1 = context.CreateToken(user, TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        // same as first but different platform
        Token token2 = context.CreateToken(user, TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3);
        // same as first but different game
        Token token3 = context.CreateToken(user, TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        // completely different game and plaftorm
        Token token4 = context.CreateToken(user, TokenType.Game, TokenGame.LittleBigPlanetVita, TokenPlatform.Vita);

        // -- Set the dates --
        // LBP1 PS3
        context.Time.TimestampMilliseconds += 5;
        context.Database.UpdateLoginDateOnUserMetric(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);

        // LBP1 RPCS3
        context.Time.TimestampMilliseconds += 10;
        context.Database.UpdateLoginDateOnUserMetric(user, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3);

        // LBP2 PS3
        context.Time.TimestampMilliseconds += 20;
        context.Database.UpdateLoginDateOnUserMetric(user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);

        // LBPVita PSVita
        context.Time.TimestampMilliseconds += 30;
        context.Database.UpdateLoginDateOnUserMetric(user, TokenGame.LittleBigPlanetVita, TokenPlatform.Vita);

        // -- Assertions --
        // LBP1 PS3
        UserGameMetric metric1 = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        Assert.That(metric1.LastLoginAt.ToUnixTimeMilliseconds(), Is.EqualTo(5));
        Assert.That(metric1.LastRoomUpdateAt, Is.EqualTo(DateTimeOffset.MinValue)); // ensure this wasn't set

        // LBP1 RPCS3
        UserGameMetric metric2 = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3);
        Assert.That(metric2.LastLoginAt.ToUnixTimeMilliseconds(), Is.EqualTo(15));
        Assert.That(metric2.LastRoomUpdateAt, Is.EqualTo(DateTimeOffset.MinValue)); // ensure this wasn't set
        
        // LBP2 PS3
        UserGameMetric metric3 = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        Assert.That(metric3.LastLoginAt.ToUnixTimeMilliseconds(), Is.EqualTo(35));
        Assert.That(metric3.LastRoomUpdateAt, Is.EqualTo(DateTimeOffset.MinValue)); // ensure this wasn't set
        
        // LBPVita PSVita
        UserGameMetric metric4 = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanetVita, TokenPlatform.Vita);
        Assert.That(metric4.LastLoginAt.ToUnixTimeMilliseconds(), Is.EqualTo(65));
        Assert.That(metric4.LastRoomUpdateAt, Is.EqualTo(DateTimeOffset.MinValue)); // ensure this wasn't set
    }
}