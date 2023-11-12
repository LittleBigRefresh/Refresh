using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Commands;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Filtering;

namespace Refresh.GameServer.Endpoints.Game;

public class ModerationEndpoints : EndpointGroup
{
    [GameEndpoint("showModeratedSlots", HttpMethods.Post, ContentType.Xml)]
    public SerializedModeratedSlotList ModerateSlots(RequestContext context, SerializedModeratedSlotList body)
    {
        return new SerializedModeratedSlotList
        {
            Ids = new List<int>(),
        };
    }

    [GameEndpoint("showModerated", HttpMethods.Post, ContentType.Xml)]
    public SerializedModeratedResourceList ModerateResources(RequestContext context, SerializedModeratedResourceList body)
    {
        return new SerializedModeratedResourceList
        {
            Resources = new List<string>(),
        };
    }

    /// <summary>
    /// Censor ("filter") strings sent by the client. Used for chat messages, speech bubble contents, etc.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <param name="commandService">The command service.</param>
    /// <param name="body">The string to censor.</param>
    /// <param name="user">The user saying the string. Used for logging and commands</param>
    /// <param name="token">The token of the user saying the string.</param>
    /// <param name="database">The database. Used for commands</param>
    /// <returns>The string shown in-game.</returns>
    [GameEndpoint("filter", HttpMethods.Post)]
    [AllowEmptyBody]
    public string Filter(RequestContext context, CommandService commandService, string body, GameUser user, Token token, GameDatabaseContext database)
    {
        // TODO: Add actual filtering/censoring
        
        if (commandService.IsPublishing(user.UserId))
        {
            context.Logger.LogInfo(BunkumCategory.UserLevels, $"Publish filter: '{body}'");
        }
        else
        {
            context.Logger.LogInfo(BunkumCategory.Filter, $"<{user}>: {body}");

            try
            {
                CommandInvocation command = commandService.ParseCommand(body);
                
                context.Logger.LogInfo(BunkumCategory.Commands, $"User used command '{command.Name.ToString()}' with args '{command.Arguments.ToString()}'");

                commandService.HandleCommand(command, database, user, token);
                return "(Command)";
            }
            catch(Exception ex)
            {
                context.Logger.LogWarning(BunkumCategory.Commands, $"Error running command {body}. ex {ex}");
                //do nothing
            } 
        }
        
        return body;
    }

    [GameEndpoint("filter/batch", HttpMethods.Post, ContentType.Xml)]
    public SerializedTextList BatchFilter(RequestContext context, SerializedTextList body)
    {
        return new SerializedTextList
        {
            AllOk = true,
        };
    }
}