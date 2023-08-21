using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer.Tests.Authentication;

public class AuthenticationIntegrationTests : GameServerTest
{
    [Test]
    public async Task GameAuthenticationWorks()
    {
        using TestContext context = this.GetServer();
        
        HttpResponseMessage unauthedRequest = await context.Http.GetAsync("/lbp/eula");
        Assert.That(unauthedRequest.StatusCode, Is.EqualTo(Forbidden));

        HttpClient authedClient = context.GetAuthenticatedClient(TokenType.Game, out string tokenData);
        
        Token? token = context.Database.GetTokenFromTokenData(tokenData, TokenType.Game);
        Assert.That(token, Is.Not.Null);
        Assert.That(token?.User, Is.Not.Null);

        HttpResponseMessage authedRequest = await authedClient.GetAsync("/lbp/eula");
        Assert.That(authedRequest.StatusCode, Is.EqualTo(OK));
    }
    
    [Test]
    public async Task ApiAuthenticationWorks()
    {
        using TestContext context = this.GetServer();
        
        HttpResponseMessage unauthedRequest = await context.Http.GetAsync("/api/v3/users/me");
        Assert.That(unauthedRequest.StatusCode, Is.EqualTo(Forbidden));

        HttpClient authedClient = context.GetAuthenticatedClient(TokenType.Api, out string tokenData);
        
        Token? token = context.Database.GetTokenFromTokenData(tokenData, TokenType.Api);
        Assert.That(token, Is.Not.Null);
        Assert.That(token?.User, Is.Not.Null);

        // TODO: Fix serialization of ObjectId
        HttpResponseMessage response = await authedClient.GetAsync("/api/v3/users/me");
        // (GameUser? user, HttpResponseMessage response) = await authedClient.GetJsonObjectAsync<GameUser>("/api/v3/user/me");
        Assert.Multiple(async () =>
        {
            // Assert.That(user, Is.Not.Null);
            Assert.That(await response.Content.ReadAsStringAsync(), Contains.Substring(token!.User.UserId.ToString()));
            Assert.That(response.StatusCode, Is.EqualTo(OK));
        });
    }

    [Test]
    public async Task TokenRefreshingWorks()
    {
        using TestContext context = this.GetServer();

        const string password = "password";

        GameUser user = context.CreateUser();
        string passwordBcrypt = BCrypt.Net.BCrypt.HashPassword(password, 4);
        context.Database.SetUserPassword(user, passwordBcrypt);

        ApiAuthenticationRequest payload = new()
        {
            EmailAddress = user.EmailAddress,
            PasswordSha512 = Encoding.ASCII.GetString(SHA512.HashData(Encoding.ASCII.GetBytes(password))),
        };

        HttpResponseMessage response = await context.Http.PostAsync("/api/v3/login", new StringContent(JsonConvert.SerializeObject(payload)));
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        

        string respString = await response.Content.ReadAsStringAsync();
        Console.WriteLine(respString);
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
        response = await context.Http.GetAsync("/api/v3/users/me");
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        // jump to when token expires
        context.Time.TimestampMilliseconds = (GameDatabaseContext.DefaultTokenExpirySeconds * 1000) + 1;
        context.Database.Refresh();
        
        Assert.Multiple(() =>
        {
            Assert.That(context.Database.GetTokenFromTokenData(authResponse!.Data!.TokenData, TokenType.Api), Is.Null);
            Assert.That(context.Database.GetTokenFromTokenData(authResponse.Data.RefreshTokenData!, TokenType.ApiRefresh), Is.Not.Null);
        });
        
        response = await context.Http.GetAsync("/api/v3/users/me");
        Assert.That(response.StatusCode, Is.EqualTo(Forbidden));

        ApiRefreshRequest refreshPayload = new()
        {
            TokenData = authResponse.Data.RefreshTokenData!,
        };

        response = await context.Http.PostAsync("/api/v3/refreshToken", new StringContent(JsonConvert.SerializeObject(refreshPayload)));
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        respString = await response.Content.ReadAsStringAsync();
        Console.WriteLine(respString);
        authResponse = JsonConvert.DeserializeObject<ApiResponse<ApiAuthenticationResponse>>(respString);

        context.Http.DefaultRequestHeaders.Remove("Authorization");
        context.Http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authResponse!.Data!.TokenData);
        response = await context.Http.GetAsync("/api/v3/users/me");
        Assert.That(response.StatusCode, Is.EqualTo(OK));
    }
}