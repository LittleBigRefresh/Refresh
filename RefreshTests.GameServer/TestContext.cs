using Bunkum.CustomHttpListener.Listeners.Direct;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer;

public record TestContext(TestRefreshGameServer Server, GameDatabaseContext Database, HttpClient Http, DirectHttpListener Listener)
{
    private int _users;
    private int UserIncrement => this._users++;

    public HttpClient GetAuthenticatedClient(TokenType type,
        GameUser? user = null,
        TokenGame game = TokenGame.Website,
        TokenPlatform platform = TokenPlatform.Website,
        int tokenExpirySeconds = GameDatabaseContext.DefaultTokenExpirySeconds)
    {
        user ??= this.CreateUser();

        Token token = this.Database.GenerateTokenForUser(user, type, game, platform, tokenExpirySeconds);
        HttpClient client = this.Listener.GetClient();

        if (type == TokenType.Game)
        {
            client.DefaultRequestHeaders.Add("Cookie", "MM_AUTH=" + token.TokenData);
        }
        else
        {
            client.DefaultRequestHeaders.Add("Authorization", token.TokenData);
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
}