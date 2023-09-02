using Bunkum.HttpServer;
using Bunkum.HttpServer.Services;
using JetBrains.Annotations;
using MongoDB.Bson;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Commands;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services;

public class CommandService : EndpointService
{
    private readonly MatchService _match;
    
    public CommandService(LoggerContainer<BunkumContext> logger, MatchService match) : base(logger) {
        this._match = match;
    }

    private readonly HashSet<ObjectId> _usersPublishing = new();

    /// <summary>
    /// Start tracking the user, eg. they started publishing
    /// </summary>
    /// <param name="id">The user ID</param>
    public void StartPublishing(ObjectId id)
    {
        //Unconditionally add the user to the set
        _ = this._usersPublishing.Add(id);
    }

    /// <summary>
    /// Stop tracking the user, eg. they stopped publishing
    /// </summary>
    /// <param name="id">The user ID</param>
    public void StopPublishing(ObjectId id)
    {
        //Unconditionally remove the user from the set
        _ = this._usersPublishing.Remove(id);
    }

    public bool IsPublishing(ObjectId id) => this._usersPublishing.Contains(id);

    /// <summary>
    /// Parse a command string into a command object
    /// </summary>
    /// <param name="str">Command string</param>
    /// <returns>Parsed command</returns>
    /// <exception cref="FormatException">When the command is in an invalid format</exception>
    [Pure]
    public Command ParseCommand(string str)
    {
        //Ensure the command string starts with a slash
        if (str[0] != '/')
        {
            throw new FormatException("Commands must start with `/`");
        }

        int idx = str.IndexOf(" ", StringComparison.Ordinal);

        //If idx is 1, the command name is blank
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (idx == 1)
        {
            throw new FormatException("Blank command name");
        }

        //If theres no space after, or if the space is the last character, then there are no arguments
        if (idx == -1 || idx == str.Length - 1)
        {
            return new Command(idx == str.Length - 1 ? str[1..idx] : str[1..], null);
        }
        
        return new Command(str[1..idx], str[(idx + 1)..]);
    }

    public void HandleCommand(Command command, GameDatabaseContext database, GameUser user)
    {
        switch (command.Name)
        {
            case "forcematch": {
                if (command.Arguments == null)
                {
                    throw new Exception("User not provided for force match command");
                }
                
                GameUser? target = database.GetUserByUsername(command.Arguments);

                if (target != null)
                {
                    this._match.SetForceMatch(user.UserId, target.UserId);
                }
                
                break;
            }
            case "clearforcematch": {
                this._match.ClearForceMatch(user.UserId);
                
                break;
            }
        }
    }
}