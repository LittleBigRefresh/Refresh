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

        // All time increments here must be whole minutes because the playtime DB method will round (or floor idk) to minutes,
        // since playtime is minutes.

        // -- Send the requests --
        // First room update.
        context.Time.TimestampMilliseconds += 1000 * 60;
        match.ExecuteMethod("CreateRoom", roomData, this.GetDataContext(context.Database, token, match), config);

        UserGameMetric metric = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        GameUser? updatedUser = context.Database.GetUserByObjectId(user.UserId);

        context.Database.Refresh();
        Assert.That(metric.LastLoginAt, Is.EqualTo(DateTimeOffset.MinValue));
        Assert.That(metric.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(1000 * 60));
        Assert.That(metric.TotalPlayTimeMinutes, Is.Zero);

        // Second room update
        context.Time.TimestampMilliseconds += 2000 * 60;
        match.ExecuteMethod("UpdateMyPlayerData", roomData, this.GetDataContext(context.Database, token, match), config);

        context.Database.Refresh();
        metric = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        updatedUser = context.Database.GetUserByObjectId(user.UserId);

        context.Database.Refresh();
        Assert.That(metric.LastLoginAt, Is.EqualTo(DateTimeOffset.MinValue));
        Assert.That(metric.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(3000 * 60));
        Assert.That(metric.TotalPlayTimeMinutes, Is.EqualTo(2));

        Assert.That(updatedUser?.Statistics, Is.Not.Null);
        Assert.That(updatedUser!.Statistics!.TotalPlayTimeMinutes, Is.EqualTo(2));

        // Update the login date
        context.Time.TimestampMilliseconds += 3000 * 60;
        context.Database.UpdateLoginDateOnUserMetric(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);

        context.Database.Refresh();
        metric = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        updatedUser = context.Database.GetUserByObjectId(user.UserId);

        context.Database.Refresh();
        Assert.That(metric.LastLoginAt.ToUnixTimeMilliseconds(), Is.EqualTo(6000 * 60)); // is now above 0
        Assert.That(metric.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(3000 * 60)); // still the same as before
        Assert.That(metric.TotalPlayTimeMinutes, Is.EqualTo(2)); // still the same as before

        Assert.That(updatedUser?.Statistics, Is.Not.Null);
        Assert.That(updatedUser!.Statistics!.TotalPlayTimeMinutes, Is.EqualTo(2)); // still the same as before

        // First room update after new login
        context.Time.TimestampMilliseconds += 4000 * 60;
        match.ExecuteMethod("UpdateMyPlayerData", roomData, this.GetDataContext(context.Database, token, match), config);

        context.Database.Refresh();
        metric = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        updatedUser = context.Database.GetUserByObjectId(user.UserId);

        context.Database.Refresh();
        Assert.That(metric.LastLoginAt.ToUnixTimeMilliseconds(), Is.EqualTo(6000 * 60)); // The same as before
        Assert.That(metric.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(10000 * 60)); // timestamp has updated
        Assert.That(metric.TotalPlayTimeMinutes, Is.EqualTo(2)); // nothing was added

        Assert.That(updatedUser?.Statistics, Is.Not.Null);
        Assert.That(updatedUser!.Statistics!.TotalPlayTimeMinutes, Is.EqualTo(2)); // was also not updated
        
        // Second room update after new login
        context.Time.TimestampMilliseconds += 6000 * 60;
        match.ExecuteMethod("UpdateMyPlayerData", roomData, this.GetDataContext(context.Database, token, match), config);

        context.Database.Refresh();
        metric = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        updatedUser = context.Database.GetUserByObjectId(user.UserId);

        context.Database.Refresh();
        Assert.That(metric.LastLoginAt.ToUnixTimeMilliseconds(), Is.EqualTo(6000 * 60)); // The same as before
        Assert.That(metric.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(16000 * 60)); // timestamp has updated
        Assert.That(metric.TotalPlayTimeMinutes, Is.EqualTo(8)); // new minutes were added

        Assert.That(updatedUser?.Statistics, Is.Not.Null);
        Assert.That(updatedUser!.Statistics!.TotalPlayTimeMinutes, Is.EqualTo(8)); // was also updated
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

        // These 3 seconds should be ignored by the playtime itself, since it's rounded to whole minutes during calculation,
        // however the dates should reflect the seconds.
        context.Time.TimestampMilliseconds += 3000;

        // -- Set the times --
        // LBP1 PS3
        match.ExecuteMethod("CreateRoom", roomData, this.GetDataContext(context.Database, token1, match), config);
        context.Time.TimestampMilliseconds += 1000 * 60;
        match.ExecuteMethod("UpdateMyPlayerData", roomData, this.GetDataContext(context.Database, token1, match), config);
        context.Database.Refresh();

        // LBP1 RPCS3
        match.ExecuteMethod("CreateRoom", roomData, this.GetDataContext(context.Database, token2, match), config);
        context.Time.TimestampMilliseconds += 2000 * 60;
        match.ExecuteMethod("UpdateMyPlayerData", roomData, this.GetDataContext(context.Database, token2, match), config);
        context.Database.Refresh();

        // LBP2 PS3
        match.ExecuteMethod("CreateRoom", roomData, this.GetDataContext(context.Database, token3, match), config);
        context.Time.TimestampMilliseconds += 6000 * 60;
        match.ExecuteMethod("UpdateMyPlayerData", roomData, this.GetDataContext(context.Database, token3, match), config);
        context.Database.Refresh();

        // LBPVita PSVita
        match.ExecuteMethod("CreateRoom", roomData, this.GetDataContext(context.Database, token4, match), config);
        context.Time.TimestampMilliseconds += 5000 * 60;
        match.ExecuteMethod("UpdateMyPlayerData", roomData, this.GetDataContext(context.Database, token4, match), config);
        context.Database.Refresh();

        // -- Assertions --
        // LBP1 PS3
        UserGameMetric metric1 = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.PS3);
        Assert.That(metric1.TotalPlayTimeMinutes, Is.EqualTo(1));
        Assert.That(metric1.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(1000 * 60 + 3000));
        
        // LBP1 RPCS3
        UserGameMetric metric2 = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3);
        Assert.That(metric2.TotalPlayTimeMinutes, Is.EqualTo(2));
        Assert.That(metric2.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(3000 * 60 + 3000));
        
        // LBP2 PS3
        UserGameMetric metric3 = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        Assert.That(metric3.TotalPlayTimeMinutes, Is.EqualTo(6));
        Assert.That(metric3.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(9000 * 60 + 3000));
        
        // LBPVita PSVita
        UserGameMetric metric4 = context.Database.GetGameMetricForUser(user, TokenGame.LittleBigPlanetVita, TokenPlatform.Vita);
        Assert.That(metric4.TotalPlayTimeMinutes, Is.EqualTo(5));
        Assert.That(metric4.LastRoomUpdateAt.ToUnixTimeMilliseconds(), Is.EqualTo(14000 * 60 + 3000));

        // Ensure the total playtime stat is actually a sum of all metric playtimes
        GameUser? userUpdated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(userUpdated?.Statistics, Is.Not.Null);
        Assert.That(userUpdated!.Statistics!.TotalPlayTimeMinutes, Is.EqualTo(14));
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