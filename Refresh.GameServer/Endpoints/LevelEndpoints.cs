using System.Net;
using System.Xml.Serialization;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints;

public class LevelEndpoints : EndpointGroup
{
    [GameEndpoint("slots/by", ContentType.Xml)]
    public string SlotsByUser(RequestContext context)
    {
        return "<slots total=\"0\" hint_start=\"0\"></slots>";
    }

    [GameEndpoint("startPublish", ContentType.Xml)]
    public GameResourceLevel? StartPublish(RequestContext context, GameUser user, RealmDatabaseContext database, string body)
    {
        Console.WriteLine(body);
        
        XmlSerializer serializer = new(typeof(GameLevel));
        GameLevel level;
        try
        {
            if (serializer.Deserialize(new StringReader(body)) is not GameLevel data) return null;
            level = data;
        }
        catch (Exception e)
        {
            context.Logger.LogError(RefreshContext.UserContent, $"Failed to parse level data: {e}\n\nXML: {body}");
            return null;
        }

        return new GameResourceLevel
        {
            Resources = level.XmlResources,
        };
    }

    [GameEndpoint("publish", ContentType.Xml)]
    public Response FinishPublishing(RequestContext context, GameUser user, RealmDatabaseContext database, string body)
    {
        Console.WriteLine(body);
        
        XmlSerializer serializer = new(typeof(GameLevel));
        GameLevel level;
        try
        {
            if (serializer.Deserialize(new StringReader(body)) is not GameLevel data) 
                return new Response(HttpStatusCode.BadRequest);
            
            level = data;
        }
        catch (Exception e)
        {
            context.Logger.LogError(RefreshContext.UserContent, $"Failed to parse level data: {e}\n\nXML: {body}");
            return new Response(HttpStatusCode.BadRequest);
        }

        level.Publisher = user;
        
        // ReSharper disable once InvertIf
        if (database.AddLevel(level))
        {
            level.PrepareForSerialization();
            return new Response(level, ContentType.Xml);
        }

        return new Response(HttpStatusCode.BadRequest);
    }
}