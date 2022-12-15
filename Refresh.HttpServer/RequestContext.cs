using System.Net;
using NotEnoughLogs;

namespace Refresh.HttpServer;

public struct RequestContext
{
    public HttpListenerRequest Request;
    public LoggerContainer<RefreshContext> Logger;
}