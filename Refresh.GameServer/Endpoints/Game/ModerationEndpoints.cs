using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Commands;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Filtering;

namespace Refresh.GameServer.Endpoints.Game;

public class ModerationEndpoints : EndpointGroup
{
    [GameEndpoint("showModeratedSlots", Method.Post, ContentType.Xml)]
    public SerializedModeratedSlotList ModerateSlots(RequestContext context, SerializedModeratedSlotList body)
    {
        return new SerializedModeratedSlotList
        {
            Ids = new List<int>(),
        };
    }
    
    /// <summary>
    /// Censor ("filter") strings sent by the client. Used for chat messages, speech bubble contents, etc.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <param name="body">The string to censor.</param>
    /// <param name="user">The user saying the string. Used for logging</param>
    /// <returns>The string shown in-game.</returns>
    [GameEndpoint("filter", Method.Post)]
    [AllowEmptyBody]
    public string Filter(RequestContext context, CommandService commandService, string body, GameUser user, GameDatabaseContext database)
    {
        // TODO: Add actual filtering/censoring
        
        if (commandService.IsPublishing(user.UserId))
        {
            context.Logger.LogInfo(BunkumContext.UserLevels, $"Publish filter: '{body}'");
        }
        else
        {
            context.Logger.LogInfo(BunkumContext.Filter, $"<{user}>: {body}");

            try
            {
                CommandInvocation command = commandService.ParseCommand(body);
                
                context.Logger.LogInfo(BunkumContext.Commands, $"User used command '{command.Name}' with args '{command.Arguments}'");

                commandService.HandleCommand(command, database, user);
            }
            catch
            {
                //do nothing
            } 
        }
        
        return body;
    }

    [GameEndpoint("filter/batch", Method.Post, ContentType.Xml)]
    public SerializedTextList BatchFilter(RequestContext context, SerializedTextList body)
    {
        return body;
    }
}