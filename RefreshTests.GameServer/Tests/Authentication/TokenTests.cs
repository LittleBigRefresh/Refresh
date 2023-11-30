using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer.Tests.Authentication;

public class TokenTests : GameServerTest
{
    [Test]
    public void TokensExpire()
    {
        using TestContext context = this.GetServer(false);
        GameUser user = context.CreateUser();

        const int expirySeconds = IGameDatabaseContext.DefaultTokenExpirySeconds;
        Token token = context.Database.GenerateTokenForUser(user, TokenType.Api, TokenGame.Website, TokenPlatform.Website, expirySeconds);
        
        Assert.That(context.Database.GetTokenFromTokenData(token.TokenData, TokenType.Api), Is.Not.Null);
        context.Time.TimestampMilliseconds = expirySeconds * 1000;
        Assert.That(context.Database.GetTokenFromTokenData(token.TokenData, TokenType.Api), Is.Not.Null);
        context.Time.TimestampMilliseconds++;
        Assert.That(context.Database.GetTokenFromTokenData(token.TokenData, TokenType.Api), Is.Null);
    }

    [Test]
    public void DoesntGetTokenWithBadTokenData()
    {
        using TestContext context = this.GetServer(false);
        
        // make a token so that the test doesnt pass cause we have no tokens
        context.Database.GenerateTokenForUser(context.CreateUser(), TokenType.Api, TokenGame.Website, TokenPlatform.Website);

        Token? token = context.Database.GetTokenFromTokenData("bad token data", TokenType.Api);
        GameUser? user = context.Database.GetUserFromTokenData("bad token data", TokenType.Api);
        
        Assert.Multiple(() =>
        {
            Assert.That(token, Is.Null);
            Assert.That(user, Is.Null);
        });
    }   
}