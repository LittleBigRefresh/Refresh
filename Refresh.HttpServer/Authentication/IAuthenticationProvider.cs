using System.Net;

namespace Refresh.HttpServer.Authentication;

public interface IAuthenticationProvider<out TUser> where TUser : IUser
{
    public TUser? AuthenticateUser(HttpListenerRequest request);
}