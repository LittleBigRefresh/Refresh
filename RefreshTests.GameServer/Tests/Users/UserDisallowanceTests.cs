using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request.Authentication;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Users;

public class UserDisallowanceTests : GameServerTest
{
    [Test]
    [TestCase("guy@lil.com")]
    [TestCase("Guy@LiL.coM")]
    [TestCase("GUY@LIL.COM")]
    public void CannotRegisterAccountWithDisallowedEmailAddressCaseInsensitively(string emailAddress)
    {
        using TestContext context = this.GetServer();

        const string emailAddressLower = "guy@lil.com";
        const string disallowReason = "being lil";
        // Not somehow already disallowed
        Assert.That(context.Database.IsEmailAddressDisallowed(emailAddress), Is.False);
        Assert.That(context.Database.GetDisallowedEmailAddressInfo(emailAddress), Is.Null);

        // Disallow
        (DisallowedEmailAddress disallowanceReturn, bool success) = context.Database.DisallowEmailAddress(emailAddress, disallowReason);
        Assert.That(disallowanceReturn.AddressLower, Is.EqualTo(emailAddressLower));
        Assert.That(disallowanceReturn.Reason, Is.EqualTo(disallowReason));
        Assert.That(success, Is.True);
        context.Database.Refresh();

        // Try to disallow again
        (disallowanceReturn, success) = context.Database.DisallowEmailAddress(emailAddress, disallowReason);
        Assert.That(disallowanceReturn.AddressLower, Is.EqualTo(emailAddressLower));
        Assert.That(disallowanceReturn.Reason, Is.EqualTo(disallowReason));
        Assert.That(success, Is.False);
        context.Database.Refresh();

        // Ensure it's actually disallowed
        Assert.That(context.Database.IsEmailAddressDisallowed(emailAddress), Is.True);
        DisallowedEmailAddress? disallowed = context.Database.GetDisallowedEmailAddressInfo(emailAddress);
        Assert.That(disallowed, Is.Not.Null);
        Assert.That(disallowed!.AddressLower, Is.EqualTo(emailAddressLower));
        Assert.That(disallowed!.Reason, Is.EqualTo(disallowReason));
        
        // Try to register
        ApiResponse<ApiAuthenticationResponse>? response = context.Http.PostData<ApiAuthenticationResponse>("/api/v3/register", new ApiRegisterRequest
        {
            Username = "a_lil_guy",
            EmailAddress = emailAddress,
            PasswordSha512 = "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff",
        }, false, true);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Error, Is.Not.Null);
        Assert.That(response.Error!.Name, Is.EqualTo("ApiAuthenticationError"));
        
        context.Database.Refresh();
        Assert.That(context.Database.GetUserByEmailAddress(emailAddress), Is.Null);

        // Undo
        success = context.Database.ReallowEmailAddress(emailAddress);
        Assert.That(success, Is.True);
        context.Database.Refresh();
        Assert.That(context.Database.IsEmailAddressDisallowed(emailAddress), Is.False);
        Assert.That(context.Database.GetDisallowedEmailAddressInfo(emailAddress), Is.Null);

        // Try to undo again
        success = context.Database.ReallowEmailAddress(emailAddress);
        Assert.That(success, Is.False);
        context.Database.Refresh();
        Assert.That(context.Database.IsEmailAddressDisallowed(emailAddress), Is.False);
        Assert.That(context.Database.GetDisallowedEmailAddressInfo(emailAddress), Is.Null);
    }

    [Test]
    [TestCase("guy@moron.com")] // whole address
    [TestCase("moron.com")] // just the domain
    [TestCase("MORON.Com")]
    [TestCase("GUY@MORoN.cOm")]
    public void CannotRegisterAccountsWithDisallowedEmailDomainCaseInsensitively(string addressToBlockWith)
    {
        using TestContext context = this.GetServer();
        const string disallowReason = "moron email moment";
        const string domain = "moron.com";

        // Not somehow already disallowed
        Assert.That(context.Database.IsEmailDomainDisallowed(addressToBlockWith), Is.False);
        Assert.That(context.Database.IsEmailDomainDisallowed(domain), Is.False);
        Assert.That(context.Database.GetDisallowedEmailDomainInfo(addressToBlockWith), Is.Null);

        // Disallow
        (DisallowedEmailDomain disallowanceReturn, bool success) = context.Database.DisallowEmailDomain(addressToBlockWith, disallowReason);
        Assert.That(disallowanceReturn.DomainLower, Is.EqualTo(domain));
        Assert.That(disallowanceReturn.Reason, Is.EqualTo(disallowReason));
        Assert.That(success, Is.True);
        context.Database.Refresh();

        // Try to disallow again
        (disallowanceReturn, success) = context.Database.DisallowEmailDomain(addressToBlockWith, disallowReason);
        Assert.That(disallowanceReturn.DomainLower, Is.EqualTo(domain));
        Assert.That(disallowanceReturn.Reason, Is.EqualTo(disallowReason));
        Assert.That(success, Is.False);
        context.Database.Refresh();

        // Ensure it's disallowed
        Assert.That(context.Database.IsEmailDomainDisallowed(addressToBlockWith), Is.True);
        Assert.That(context.Database.IsEmailDomainDisallowed(domain), Is.True);
        DisallowedEmailDomain? disallowed = context.Database.GetDisallowedEmailDomainInfo(addressToBlockWith);
        Assert.That(disallowed, Is.Not.Null);
        Assert.That(disallowed!.DomainLower, Is.EqualTo(domain));
        Assert.That(disallowed!.Reason, Is.EqualTo(disallowReason));
        
        // Attempt 1 (block)
        ApiResponse<ApiAuthenticationResponse>? response = context.Http.PostData<ApiAuthenticationResponse>("/api/v3/register", new ApiRegisterRequest
        {
            Username = "a_lil_guy",
            EmailAddress = "pisser@moron.com",
            PasswordSha512 = "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff",
        }, false, true);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Error, Is.Not.Null);
        Assert.That(response.Error!.Name, Is.EqualTo("ApiAuthenticationError"));
        context.Database.Refresh();
        Assert.That(context.Database.GetUserByEmailAddress("pisser@moron.com"), Is.Null);

        // Attempt 2 (block)
        response = context.Http.PostData<ApiAuthenticationResponse>("/api/v3/register", new ApiRegisterRequest
        {
            Username = "a_lil_guy",
            EmailAddress = "shitter@moron.com",
            PasswordSha512 = "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff",
        }, false, true);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Error, Is.Not.Null);
        Assert.That(response.Error!.Name, Is.EqualTo("ApiAuthenticationError"));
        context.Database.Refresh();
        Assert.That(context.Database.GetUserByEmailAddress("shitter@moron.com"), Is.Null);

        // Attempt 3 (block)
        response = context.Http.PostData<ApiAuthenticationResponse>("/api/v3/register", new ApiRegisterRequest
        {
            Username = "a_lil_guy",
            EmailAddress = ".@moron.com",
            PasswordSha512 = "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff",
        }, false, true);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Error, Is.Not.Null);
        Assert.That(response.Error!.Name, Is.EqualTo("ApiAuthenticationError"));
        context.Database.Refresh();
        Assert.That(context.Database.GetUserByEmailAddress(".@moron.com"), Is.Null);

        // Attempt 4 (allow)
        response = context.Http.PostData<ApiAuthenticationResponse>("/api/v3/register", new ApiRegisterRequest
        {
            Username = "a_lil_guy",
            EmailAddress = "quacker@hi.com",
            PasswordSha512 = "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff",
        });
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Error, Is.Null);
        Assert.That(response!.Data, Is.Not.Null);
        context.Database.Refresh();

        // Ensure Quacker has successfully registered and hasn't been blocked
        GameUser? quacker = context.Database.GetUserByEmailAddress("quacker@hi.com");
        Assert.That(quacker, Is.Not.Null);
        Assert.That(quacker!.UserId.ToString(), Is.EqualTo(response.Data!.UserId));
        Assert.That(quacker!.Username, Is.EqualTo("a_lil_guy"));

        // Undo
        success = context.Database.ReallowEmailDomain(addressToBlockWith);
        Assert.That(success, Is.True);
        context.Database.Refresh();
        Assert.That(context.Database.IsEmailDomainDisallowed(addressToBlockWith), Is.False);
        Assert.That(context.Database.IsEmailDomainDisallowed(domain), Is.False);
        Assert.That(context.Database.GetDisallowedEmailDomainInfo(addressToBlockWith), Is.Null);

        // Try to undo again
        success = context.Database.ReallowEmailDomain(addressToBlockWith);
        Assert.That(success, Is.False);
        context.Database.Refresh();
        Assert.That(context.Database.IsEmailDomainDisallowed(addressToBlockWith), Is.False);
        Assert.That(context.Database.IsEmailDomainDisallowed(domain), Is.False);
        Assert.That(context.Database.GetDisallowedEmailDomainInfo(addressToBlockWith), Is.Null);
    }
    
    [Test]
    [TestCase("a_lil_guy")]
    [TestCase("a_LiL_guY")]
    [TestCase("A_LIL_GUY")]
    public void CannotRegisterAccountWithDisallowedUsernameCaseInsensitively(string username)
    {
        using TestContext context = this.GetServer();
        const string usernameLower = "a_lil_guy";
        const string disallowReason = "writing these is fun lol";

        // Not somehow already disallowed
        Assert.That(context.Database.IsUserDisallowed(username), Is.False);
        Assert.That(context.Database.GetDisallowedUserInfo(username), Is.Null);

        // Disallow
        (DisallowedUser disallowanceReturn, bool success) = context.Database.DisallowUser(username, disallowReason);
        Assert.That(disallowanceReturn.UsernameLower, Is.EqualTo(usernameLower));
        Assert.That(disallowanceReturn.Reason, Is.EqualTo(disallowReason));
        Assert.That(success, Is.True);
        context.Database.Refresh();

        // Try to disallow again
        (disallowanceReturn, success) = context.Database.DisallowUser(username, disallowReason);
        Assert.That(disallowanceReturn.UsernameLower, Is.EqualTo(usernameLower));
        Assert.That(disallowanceReturn.Reason, Is.EqualTo(disallowReason));
        Assert.That(success, Is.False);
        context.Database.Refresh();

        // Ensure it's disallowed
        Assert.That(context.Database.IsUserDisallowed(username), Is.True);
        DisallowedUser? disallowed = context.Database.GetDisallowedUserInfo(username);
        Assert.That(disallowed, Is.Not.Null);
        Assert.That(disallowed!.UsernameLower, Is.EqualTo(usernameLower));
        Assert.That(disallowed!.Reason, Is.EqualTo(disallowReason));
        
        // Try to register
        ApiResponse<ApiAuthenticationResponse>? response = context.Http.PostData<ApiAuthenticationResponse>("/api/v3/register", new ApiRegisterRequest
        {
            Username = username,
            EmailAddress = "guy@lil.com",
            PasswordSha512 = "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff",
        }, false, true);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Error, Is.Not.EqualTo(null));
        Assert.That(response.Error!.Name, Is.EqualTo("ApiAuthenticationError"));

        // Ensure registration actually failed
        context.Database.Refresh();
        Assert.That(context.Database.GetUserByUsername(username), Is.Null);
        
        // Undo
        success = context.Database.ReallowUser(username);
        Assert.That(success, Is.True);
        context.Database.Refresh();
        Assert.That(context.Database.IsUserDisallowed(username), Is.False);
        Assert.That(context.Database.GetDisallowedUserInfo(username), Is.Null);

        // Try to undo again
        success = context.Database.ReallowUser(username);
        Assert.That(success, Is.False);
        context.Database.Refresh();
        Assert.That(context.Database.IsUserDisallowed(username), Is.False);
        Assert.That(context.Database.GetDisallowedUserInfo(username), Is.Null);
    }
}