using System.Net;
using Refresh.HttpServer.Authentication;
using Refresh.HttpServer.Authentication.Dummy;

namespace RefreshTests.HttpServer.Authentication;

public class CallbackAuthenticationProvider : DummyAuthenticationProvider
{
    private readonly Action _action;

    public CallbackAuthenticationProvider(Action action)
    {
        this._action = action;
    }

    public override DummyUser? AuthenticateUser(HttpListenerRequest request)
    {
        this._action.Invoke();
        return base.AuthenticateUser(request);
    }
}