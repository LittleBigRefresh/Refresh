using System.Net;
using Refresh.HttpServer.Authentication.Dummy;
using Refresh.HttpServer.Database;

namespace RefreshTests.HttpServer.Authentication;

public class CallbackAuthenticationProvider : DummyAuthenticationProvider
{
    private readonly Action _action;

    public CallbackAuthenticationProvider(Action action)
    {
        this._action = action;
    }

    public override DummyUser? AuthenticateUser(HttpListenerRequest request, IDatabaseContext database)
    {
        this._action.Invoke();
        return base.AuthenticateUser(request, database);
    }
}