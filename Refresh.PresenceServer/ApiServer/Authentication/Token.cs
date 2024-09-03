using Bunkum.Core.Authentication;

namespace Refresh.PresenceServer.ApiServer.Authentication;

public class Token : IToken<IUser>
{
    public IUser User => null!;
}