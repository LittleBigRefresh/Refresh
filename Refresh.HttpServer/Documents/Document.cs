using System.Diagnostics.Contracts;
using System.Net;
using Refresh.HttpServer.Responses;

namespace Refresh.HttpServer.Documents;

public abstract class Document
{
    public abstract string Name { get; }

    [Pure]
    public abstract Response GetResponse(HttpListenerContext context);
}