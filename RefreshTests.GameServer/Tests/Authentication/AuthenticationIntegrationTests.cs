using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users;
using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer.Tests.Authentication;

public class AuthenticationIntegrationTests : GameServerTest
{
    [Test]
    public void GameAuthenticationWorks()
    {
        using TestContext context = this.GetServer();
        
        HttpResponseMessage unauthedRequest = context.Http.GetAsync("/lbp/eula").Result;
        Assert.That(unauthedRequest.StatusCode, Is.EqualTo(Forbidden));

        HttpClient authedClient = context.GetAuthenticatedClient(TokenType.Game, out string tokenData);
        
        Token? token = context.Database.GetTokenFromTokenData(tokenData, TokenType.Game);
        Assert.That(token, Is.Not.Null);
        Assert.That(token?.User, Is.Not.Null);

        HttpResponseMessage authedRequest = authedClient.GetAsync("/lbp/eula").Result;
        Assert.That(authedRequest.StatusCode, Is.EqualTo(OK));
    }
    
    [Test]
    public void ApiAuthenticationWorks()
    {
        using TestContext context = this.GetServer();
        
        HttpResponseMessage unauthedRequest = context.Http.GetAsync("/api/v3/users/me").Result;
        Assert.That(unauthedRequest.StatusCode, Is.EqualTo(Forbidden));

        HttpClient authedClient = context.GetAuthenticatedClient(TokenType.Api, out string tokenData);
        
        Token? token = context.Database.GetTokenFromTokenData(tokenData, TokenType.Api);
        Assert.That(token, Is.Not.Null);
        Assert.That(token?.User, Is.Not.Null);

        // TODO: Fix serialization of ObjectId
        HttpResponseMessage response = authedClient.GetAsync("/api/v3/users/me").Result;
        // (GameUser? user, HttpResponseMessage response) = authedClient.GetJsonObjectAsync<GameUser>("/api/v3/user/me");
        Assert.Multiple(async () =>
        {
            Assert.That(await response.Content.ReadAsStringAsync(), Contains.Substring(token!.User.UserId.ToString()));
            Assert.That(response.StatusCode, Is.EqualTo(OK));
        });
    }

    [Test]
    public void TokenRefreshingWorks()
    {
        using TestContext context = this.GetServer();

        const string password = "password";

        GameUser user = context.CreateUser();
        string passwordBcrypt = BCrypt.Net.BCrypt.HashPassword(password, 4);
        context.Database.SetUserPassword(user, passwordBcrypt);

        ApiAuthenticationRequest payload = new()
        {
            EmailAddress = user.EmailAddress,
            PasswordSha512 = password,
        };

        HttpResponseMessage response = context.Http.PostAsync("/api/v3/login", new StringContent(JsonConvert.SerializeObject(payload))).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        string respString = response.Content.ReadAsStringAsync().Result;
        context.Server.Value.Logger.LogTrace("Tests", respString);
        ApiResponse<ApiAuthenticationResponse>? authResponse = JsonConvert.DeserializeObject<ApiResponse<ApiAuthenticationResponse>>(respString);
        Assert.Multiple(() =>
        {
            Assert.That(authResponse, Is.Not.Null);
            Assert.That(authResponse!.Success, Is.True);
            Assert.That(authResponse.Data, Is.Not.Null);
            Assert.That(authResponse.Data!.TokenData, Is.Not.Null);
            Assert.That(authResponse.Data.TokenData, Is.Not.Empty);
        });

        context.Database.Refresh();
        Assert.Multiple(() =>
        {
            Assert.That(context.Database.GetTokenFromTokenData(authResponse!.Data!.TokenData, TokenType.Api), Is.Not.Null);
            Assert.That(context.Database.GetTokenFromTokenData(authResponse.Data.RefreshTokenData!, TokenType.ApiRefresh), Is.Not.Null);
        });

        context.Http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authResponse!.Data!.TokenData);
        response = context.Http.GetAsync("/api/v3/users/me").Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        // jump to when token expires
        context.Time.TimestampMilliseconds = (GameDatabaseContext.DefaultTokenExpirySeconds * 1000) + 1;
        context.Database.Refresh();

        Assert.Multiple(() =>
        {
            Assert.That(context.Database.GetTokenFromTokenData(authResponse!.Data!.TokenData, TokenType.Api), Is.Null);
            Assert.That(context.Database.GetTokenFromTokenData(authResponse.Data.RefreshTokenData!, TokenType.ApiRefresh), Is.Not.Null);
        });

        response = context.Http.GetAsync("/api/v3/users/me").Result;
        Assert.That(response.StatusCode, Is.EqualTo(Forbidden));

        ApiRefreshRequest refreshPayload = new()
        {
            TokenData = authResponse.Data.RefreshTokenData!,
        };

        response = context.Http.PostAsync("/api/v3/refreshToken", new StringContent(JsonConvert.SerializeObject(refreshPayload))).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        respString = response.Content.ReadAsStringAsync().Result;
        context.Server.Value.Logger.LogTrace("Tests", respString);
        authResponse = JsonConvert.DeserializeObject<ApiResponse<ApiAuthenticationResponse>>(respString);

        context.Http.DefaultRequestHeaders.Remove("Authorization");
        context.Http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authResponse!.Data!.TokenData);
        response = context.Http.GetAsync("/api/v3/users/me").Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
    }
}