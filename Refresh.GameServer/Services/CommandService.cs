using System.Diagnostics.CodeAnalysis;
using Bunkum.Core.Services;
using JetBrains.Annotations;
using MongoDB.Bson;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Commands;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services;

public class CommandService : EndpointService
{
    private readonly MatchService _match;
    private readonly LevelListOverrideService _levelListService;
    
    public CommandService(Logger logger, MatchService match, LevelListOverrideService levelListService) : base(logger) {
        this._match = match;
        this._levelListService = levelListService;
    }

    private readonly HashSet<ObjectId> _usersPublishing = new();

    /// <summary>
    /// Start tracking the user, eg. they started publishing
    /// </summary>
    /// <param name="id">The user ID</param>
    public void StartPublishing(ObjectId id)
    {
        //Unconditionally add the user to the set
        this._usersPublishing.Add(id);
    }

    /// <summary>
    /// Stop tracking the user, eg. they stopped publishing
    /// </summary>
    /// <param name="id">The user ID</param>
    public void StopPublishing(ObjectId id)
    {
        //Unconditionally remove the user from the set
        this._usersPublishing.Remove(id);
    }

    public bool IsPublishing(ObjectId id) => this._usersPublishing.Contains(id);

    /// <summary>
    /// Parse a command string into a command object
    /// </summary>
    /// <param name="input">Command string</param>
    /// <returns>Parsed command</returns>
    /// <exception cref="FormatException">When the command is in an invalid format</exception>
    [Pure]
    public CommandInvocation ParseCommand(ReadOnlySpan<char> input)
    {
        // Ensure the command string starts with a slash
        if (input[0] != '/')
        {
            throw new FormatException("Commands must start with `/`");
        }

        int index = input.IndexOf(' ');

        // If index is 1, the command name is blank
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (index == 1)
        {
            throw new FormatException("Blank command name");
        }

        //If theres no space after, or if the space is the last character, then there are no arguments
        if (index == -1 || index == input.Length - 1)
        {
            return new CommandInvocation(index == input.Length - 1 ? input[1..index] : input[1..], null);
        }
        
        return new CommandInvocation(input[1..index], input[(index + 1)..]);
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public void HandleCommand(CommandInvocation command, IGameDatabaseContext database, GameUser user, Token token)
    {
        switch (command.Name)
        {
            case "forcematch":
            {
                if (command.Arguments == null)
                {
                    throw new Exception("User not provided for force match command");
                }
                
                GameUser? target = database.GetUserByUsername(command.Arguments.ToString());

                if (target != null)
                {
                    this._match.SetForceMatch(user.UserId, target.UserId);
                }
                
                break;
            }
            case "clearforcematch":
            {
                this._match.ClearForceMatch(user.UserId);
                break;
            }
            case "griefphotoson":
            {
                database.SetUserGriefReportRedirection(user, true);
                break;
            }
            case "griefphotosoff":
            {
                database.SetUserGriefReportRedirection(user, false);
                break;
            }
            case "play":
            {
                GameLevel? level = database.GetLevelById(int.Parse(command.Arguments));
                if (level != null)
                {
                    this._levelListService.AddOverridesForUser(user, level);
                }
                break;
            }
            #if DEBUG
            case "tokengame":
            {
                database.ForceUserTokenGame(token, (TokenGame)int.Parse(command.Arguments));
                break;
            }
            case "tokenplatform":
            {
                database.ForceUserTokenPlatform(token, (TokenPlatform)int.Parse(command.Arguments));
                break;
            }
            #endif
        }
    }
}