using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using RefreshTests.GameServer.Extensions;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Assets;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request.Moderation;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Disallowed;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class DisallowanceApiTests : GameServerTest
{
    [Test]
    [TestCase(GameAssetType.Plan)]
    [TestCase(null)]
    public void DisallowGetAndReallowAssetHash(GameAssetType? type)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser(role: GameUserRole.Moderator);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        string hash = "lel";
        string typeStr = (type ?? GameAssetType.Unknown).ToString();

        // Ensure it's not already there
        Assert.That(context.Database.GetDisallowedAssetInfo(hash), Is.Null);
        ApiListResponse<ApiDisallowedAssetResponse>? disallowedList = client.GetList<ApiDisallowedAssetResponse>("/api/v3/admin/disallowed/assetHashes");
        Assert.That(disallowedList?.Data, Is.Not.Null);
        Assert.That(disallowedList!.Data, Is.Empty);
        
        // Create
        ApiDisallowAssetRequest request = new()
        {
            Type = typeStr,
            Reason = "making these up surely never gets boring",
        };

        ApiResponse<ApiDisallowedAssetResponse>? response = client.PostData<ApiDisallowedAssetResponse>($"/api/v3/admin/disallowed/assetHashes/hash/{hash}", request, false);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.AssetHash, Is.EqualTo(hash));
        Assert.That(response!.Data!.AssetType, Is.EqualTo(request.Type));
        Assert.That(response!.Data!.Reason, Is.EqualTo(request.Reason));

        context.Database.Refresh();

        // Try to create again
        response = client.PostData<ApiDisallowedAssetResponse>($"/api/v3/admin/disallowed/assetHashes/hash/{hash}", request, false);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.AssetHash, Is.EqualTo(hash));
        Assert.That(response!.Data!.AssetType, Is.EqualTo(request.Type));
        Assert.That(response!.Data!.Reason, Is.EqualTo(request.Reason));

        context.Database.Refresh();

        // Ensure it now appears in listings (both unfiltered and filtered)
        Assert.That(context.Database.GetDisallowedAssetInfo(hash), Is.Not.Null);
        disallowedList = client.GetList<ApiDisallowedAssetResponse>($"/api/v3/admin/disallowed/assetHashes");
        Assert.That(disallowedList?.Data, Is.Not.Null);
        Assert.That(disallowedList!.Data!.Count(), Is.EqualTo(1));
        Assert.That(disallowedList!.Data![0].AssetHash, Is.EqualTo(hash));

        disallowedList = client.GetList<ApiDisallowedAssetResponse>($"/api/v3/admin/disallowed/assetHashes?type={typeStr}");
        Assert.That(disallowedList?.Data, Is.Not.Null);
        Assert.That(disallowedList!.Data!.Count(), Is.EqualTo(1));
        Assert.That(disallowedList!.Data![0].AssetHash, Is.EqualTo(hash));

        // Remove
        client.DeleteData<ApiDisallowedAssetResponse>($"/api/v3/admin/disallowed/assetHashes/hash/{hash}", request);
        context.Database.Refresh();

        // Ensure it's no longer there
        Assert.That(context.Database.GetDisallowedAssetInfo(hash), Is.Null);
        disallowedList = client.GetList<ApiDisallowedAssetResponse>($"/api/v3/admin/disallowed/assetHashes");
        Assert.That(disallowedList?.Data, Is.Not.Null);
        Assert.That(disallowedList!.Data, Is.Empty);
    }

    [Test]
    public void DisallowGetAndReallowUsername()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser(role: GameUserRole.Moderator);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        string name = "your";

        // Ensure it's not already there
        Assert.That(context.Database.IsUserDisallowed(name), Is.False);
        ApiListResponse<ApiDisallowedUsernameResponse>? disallowedList = client.GetList<ApiDisallowedUsernameResponse>("/api/v3/admin/disallowed/usernames");
        Assert.That(disallowedList?.Data, Is.Not.Null);
        Assert.That(disallowedList!.Data, Is.Empty);
        
        // Create
        ApiModerationRequest request = new()
        {
            Reason = "long",
        };

        ApiResponse<ApiDisallowedUsernameResponse>? response = client.PostData<ApiDisallowedUsernameResponse>($"/api/v3/admin/disallowed/usernames/name/{name}", request, false);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Username, Is.EqualTo(name));
        Assert.That(response!.Data!.Reason, Is.EqualTo(request.Reason));

        context.Database.Refresh();

        // Try to create again
        response = client.PostData<ApiDisallowedUsernameResponse>($"/api/v3/admin/disallowed/usernames/name/{name}", request, false);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Username, Is.EqualTo(name));
        Assert.That(response!.Data!.Reason, Is.EqualTo(request.Reason));

        context.Database.Refresh();

        // Ensure it now appears in listings
        Assert.That(context.Database.IsUserDisallowed(name), Is.True);
        disallowedList = client.GetList<ApiDisallowedUsernameResponse>($"/api/v3/admin/disallowed/usernames");
        Assert.That(disallowedList?.Data, Is.Not.Null);
        Assert.That(disallowedList!.Data!.Count(), Is.EqualTo(1));
        Assert.That(disallowedList!.Data![0].Username, Is.EqualTo(name));

        // Remove
        client.DeleteData<ApiDisallowedAssetResponse>($"/api/v3/admin/disallowed/usernames/name/{name}", request);
        context.Database.Refresh();

        // Ensure it's no longer there
        Assert.That(context.Database.IsUserDisallowed(name), Is.False);
        disallowedList = client.GetList<ApiDisallowedUsernameResponse>($"/api/v3/admin/disallowed/usernames");
        Assert.That(disallowedList?.Data, Is.Not.Null);
        Assert.That(disallowedList!.Data, Is.Empty);
    }

    [Test]
    public void DisallowGetAndReallowEmailAddress()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser(role: GameUserRole.Moderator);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        string address = "thefunny@brickshitter.real";

        // Ensure it's not already there
        Assert.That(context.Database.IsUserDisallowed(address), Is.False);
        ApiListResponse<ApiDisallowedEmailAddressResponse>? disallowedList = client.GetList<ApiDisallowedEmailAddressResponse>("/api/v3/admin/disallowed/emailAddresses");
        Assert.That(disallowedList?.Data, Is.Not.Null);
        Assert.That(disallowedList!.Data, Is.Empty);
        
        // Create
        ApiModerationRequest request = new()
        {
            Reason = "inapprop",
        };

        ApiResponse<ApiDisallowedEmailAddressResponse>? response = client.PostData<ApiDisallowedEmailAddressResponse>($"/api/v3/admin/disallowed/emailAddresses/address/{address}", request, false);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Address, Is.EqualTo(address));
        Assert.That(response!.Data!.Reason, Is.EqualTo(request.Reason));

        context.Database.Refresh();

        // Try to create again
        response = client.PostData<ApiDisallowedEmailAddressResponse>($"/api/v3/admin/disallowed/emailAddresses/address/{address}", request, false);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Address, Is.EqualTo(address));
        Assert.That(response!.Data!.Reason, Is.EqualTo(request.Reason));

        context.Database.Refresh();

        // Ensure it now appears in listings
        Assert.That(context.Database.IsUserDisallowed(address), Is.True);
        disallowedList = client.GetList<ApiDisallowedEmailAddressResponse>($"/api/v3/admin/disallowed/emailAddresses");
        Assert.That(disallowedList?.Data, Is.Not.Null);
        Assert.That(disallowedList!.Data!.Count(), Is.EqualTo(1));
        Assert.That(disallowedList!.Data![0].Address, Is.EqualTo(address));

        // Remove
        client.DeleteData<ApiDisallowedAssetResponse>($"/api/v3/admin/disallowed/emailAddresses/address/{address}", request);
        context.Database.Refresh();

        // Ensure it's no longer there
        Assert.That(context.Database.IsUserDisallowed(address), Is.False);
        disallowedList = client.GetList<ApiDisallowedEmailAddressResponse>($"/api/v3/admin/disallowed/emailAddresses");
        Assert.That(disallowedList?.Data, Is.Not.Null);
        Assert.That(disallowedList!.Data, Is.Empty);
    }

    [Test]
    public void DisallowGetAndReallowEmailDomain()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser(role: GameUserRole.Moderator);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        string address = "hi@brickshitter.real";
        string domain = "brickshitter.real";

        // Ensure it's not already there
        Assert.That(context.Database.IsEmailDomainDisallowed(address), Is.False);
        ApiListResponse<ApiDisallowedEmailDomainResponse>? disallowedList = client.GetList<ApiDisallowedEmailDomainResponse>("/api/v3/admin/disallowed/emailDomains");
        Assert.That(disallowedList?.Data, Is.Not.Null);
        Assert.That(disallowedList!.Data, Is.Empty);
        
        // Create
        ApiModerationRequest request = new()
        {
            Reason = "inapprop",
        };

        ApiResponse<ApiDisallowedEmailDomainResponse>? response = client.PostData<ApiDisallowedEmailDomainResponse>($"/api/v3/admin/disallowed/emailDomains/domain/{address}", request, false);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Domain, Is.EqualTo(domain));
        Assert.That(response!.Data!.Reason, Is.EqualTo(request.Reason));

        context.Database.Refresh();

        // Try to create again
        response = client.PostData<ApiDisallowedEmailDomainResponse>($"/api/v3/admin/disallowed/emailDomains/domain/{address}", request, false);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Domain, Is.EqualTo(domain));
        Assert.That(response!.Data!.Reason, Is.EqualTo(request.Reason));

        context.Database.Refresh();

        // Ensure it now appears in listings
        Assert.That(context.Database.IsEmailDomainDisallowed(address), Is.True);
        disallowedList = client.GetList<ApiDisallowedEmailDomainResponse>($"/api/v3/admin/disallowed/emailDomains");
        Assert.That(disallowedList?.Data, Is.Not.Null);
        Assert.That(disallowedList!.Data!.Count(), Is.EqualTo(1));
        Assert.That(disallowedList!.Data![0].Domain, Is.EqualTo(domain));

        // Remove
        client.DeleteData<ApiDisallowedAssetResponse>($"/api/v3/admin/disallowed/emailDomains/domain/{address}", request);
        context.Database.Refresh();

        // Ensure it's no longer there
        Assert.That(context.Database.IsEmailDomainDisallowed(address), Is.False);
        disallowedList = client.GetList<ApiDisallowedEmailDomainResponse>($"/api/v3/admin/disallowed/emailDomains");
        Assert.That(disallowedList?.Data, Is.Not.Null);
        Assert.That(disallowedList!.Data, Is.Empty);
    }
}