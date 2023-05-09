using Bunkum.CustomHttpListener.Listeners.Direct;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace RefreshTests.GameServer;

public record TestContext(TestRefreshGameServer Server, GameDatabaseContext Database, HttpClient Http, DirectHttpListener Listener)
{
    private int _users;
    private int UserIncrement => this._users++;

    public HttpClient GetAuthenticatedClient(TokenType type,
        GameUser? user = null,
        int tokenExpirySeconds = GameDatabaseContext.DefaultTokenExpirySeconds)
    {
        return this.GetAuthenticatedClient(type, out _, user, tokenExpirySeconds);
    }

    public HttpClient GetAuthenticatedClient(TokenType type, out string tokenData,
        GameUser? user = null,
        int tokenExpirySeconds = GameDatabaseContext.DefaultTokenExpirySeconds)
    {
        user ??= this.CreateUser();

        TokenGame game = type switch
        {
            TokenType.Game => TokenGame.LittleBigPlanet2,
            _ => TokenGame.Website,
        };

        TokenPlatform platform = type switch
        {
            TokenType.Game => TokenPlatform.PS3,
            _ => TokenPlatform.Website,
        };

        Token token = this.Database.GenerateTokenForUser(user, type, game, platform, tokenExpirySeconds);
        tokenData = token.TokenData;
        
        HttpClient client = this.Listener.GetClient();

        if (type == TokenType.Game)
        {
            client.DefaultRequestHeaders.Add("Cookie", "MM_AUTH=" + token.TokenData);
        }
        else
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token.TokenData);
        }

        return client;
    }
    
    public GameUser CreateUser(string? username = null)
    {
        username ??= this.UserIncrement.ToString();
        return this.Database.CreateUser(username);
    }
    
    public GameLevel CreateLevel(GameUser author, string title = "Level")
    {
        GameLevel level = new()
        {
            Title = title,
            Publisher = author,
            Location = GameLocation.Zero,
        };

        this.Database.AddLevel(level);
        return level;
    }

    public void FillLeaderboard(GameLevel level, int count)
    {
        for (byte i = 0; i < count; i++)
        {
            GameUser scoreUser = this.Database.CreateUser("score" + i);
            this.SubmitScore(i, level, scoreUser);
        }
    }

    public GameSubmittedScore SubmitScore(int score, GameLevel level, GameUser user)
    {
        GameScore scoreObject = new()
        {
            Host = true,
            Score = score,
            ScoreType = 1,
        };
        
        GameSubmittedScore? submittedScore = this.Database.SubmitScore(scoreObject, user, level);
        Assert.That(submittedScore, Is.Not.Null);

        return submittedScore!;
    }
}