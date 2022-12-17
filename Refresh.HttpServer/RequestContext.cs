using System.Net;
using NotEnoughLogs;
using Refresh.HttpServer.Storage;

namespace Refresh.HttpServer;

public struct RequestContext
{
    public HttpListenerRequest Request;
    public LoggerContainer<RefreshContext> Logger;
    public IDataStore DataStore;
}