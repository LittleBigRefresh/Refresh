using System.Net;

namespace Refresh.HttpServer.Authentication.Dummy;

public class DummyAuthenticationProvider : IAuthenticationProvider<DummyUser>
{
    public DummyUser? AuthenticateUser(HttpListenerRequest request) => 
        request.Headers["dummy-skip-auth"] != null ? null : new DummyUser();
}