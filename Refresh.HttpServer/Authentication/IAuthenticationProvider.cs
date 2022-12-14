using System.Net;
using Refresh.HttpServer.Database;

namespace Refresh.HttpServer.Authentication;

public interface IAuthenticationProvider<out TUser> where TUser : IUser
{
    // TODO: this is sloppy, figure out how to let auth providers (optionally) choose their own database context
    public TUser? AuthenticateUser(HttpListenerRequest request, IDatabaseContext database);
}