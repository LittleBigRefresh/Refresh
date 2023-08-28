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
    public Response UploadReport(RequestContext context, GameDatabaseContext database, GameReport body)
    {
        if ((body.LevelId != 0 && database.GetLevelById(body.LevelId) == null) || body.Players.Length > 4 || body.ScreenElements.Player.Length > 4)
        {
            return BadRequest;
        }
        
        database.AddGriefReport(body);
        
        return OK;
    }
}