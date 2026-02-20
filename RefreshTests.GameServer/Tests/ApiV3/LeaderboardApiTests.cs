using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class LeaderboardApiTests : GameServerTest
{
    [Test]
    public async Task DeletesScoresByPublisherUuidAndName()
    {
        using TestContext context = this.GetServer();
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator);
        GameLevel level = context.CreateLevel(mod);
        GameUser publisher = context.CreateUser(role: GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);

        // UUID
        context.SubmitScore(4000001, 1, level, publisher, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3, [publisher]);
        Assert.That(context.Database.GetTopScoresForLevel(level, 100, 0, 1).TotalItems, Is.EqualTo(1)); // TODO: Use total score by publisher
        HttpResponseMessage resetResponse = await client.DeleteAsync($"/api/v3/admin/users/uuid/{publisher.UserId}/scores");
        Assert.That(resetResponse.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetTopScoresForLevel(level, 100, 0, 1).TotalItems, Is.Zero);

        // name
        context.SubmitScore(4000001, 1, level, publisher, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3, [publisher]);
        Assert.That(context.Database.GetTopScoresForLevel(level, 100, 0, 1).TotalItems, Is.EqualTo(1));
        resetResponse = await client.DeleteAsync($"/api/v3/admin/users/name/{publisher.Username}/scores");
        Assert.That(resetResponse.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetTopScoresForLevel(level, 100, 0, 1).TotalItems, Is.Zero);
    }
}