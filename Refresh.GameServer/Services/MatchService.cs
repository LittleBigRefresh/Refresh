using System.Diagnostics;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;
using System.Net;
using System.Reflection;
using Bunkum.HttpServer.Responses;
using Newtonsoft.Json;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.Matching.MatchMethods;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services;

public partial class MatchService : EndpointService
{
    private readonly List<IMatchMethod> _matchMethods = new();

    private readonly List<GameRoom> _rooms = new();

    // unsure if this can be casted back into a list, prolly fine tho, we're not a library
    public IReadOnlyCollection<GameRoom> Rooms => this._rooms.AsReadOnly();

    public MatchService(LoggerContainer<BunkumContext> logger) : base(logger)
    {}

    public GameRoom GetOrCreateRoomByPlayer(GameDatabaseContext database, GameUser player)
    {
        GameRoom? room = this.GetRoomByPlayer(database, player);

        // ReSharper disable once InvertIf (happy path goes last)
        if (room == null)
        {
            room = new GameRoom(player);
            this._rooms.Add(room);
        }

        return room;
    }

    public GameRoom? GetRoomByPlayer(GameDatabaseContext database, GameUser player) => this._rooms.FirstOrDefault(r => r.GetPlayers(database).Contains(player));

    public override void Initialize()
    {
        // TODO: discover match methods via source generation
        List<Type> matchMethodTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IMatchMethod)) && t != typeof(IMatchMethod))
            .ToList();

        foreach (Type type in matchMethodTypes)
        {
            string name = type.Name.Substring(0, type.Name.IndexOf("Method", StringComparison.Ordinal));
            this.Logger.LogTrace(BunkumContext.Service, $"Found {nameof(IMatchMethod)} '{name}'");
            
            this._matchMethods.Add((IMatchMethod)Activator.CreateInstance(type)!);
        }
        
        this.Logger.LogDebug(BunkumContext.Service, $"Discovered {matchMethodTypes.Count} match method types");
    }

    private IMatchMethod? TryGetMatchMethod(string method) 
        => this._matchMethods.FirstOrDefault(m => m.MethodNames.Contains(method));

    public Response ExecuteMethod(string methodStr, SerializedRoomData roomData, GameDatabaseContext database, GameUser user)
    {
        IMatchMethod? method = this.TryGetMatchMethod(methodStr);
        if (method == null) return HttpStatusCode.BadRequest;

        return method.Execute(this, this.Logger, database, user, roomData);
    }
}