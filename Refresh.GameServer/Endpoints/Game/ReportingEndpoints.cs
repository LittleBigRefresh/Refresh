using System.Net;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Report;

namespace Refresh.GameServer.Endpoints.Game; 

public class ReportingEndpoints : EndpointGroup 
{
    [GameEndpoint("grief", Method.Post, ContentType.Xml)]
    public Response UploadResource(RequestContext context, RealmDatabaseContext database, GriefReport body) 
    {
        database.AddGriefReport(body);
        
        return new Response(HttpStatusCode.OK);
    }
}