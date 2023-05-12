using Bunkum.HttpServer;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;
using System.Net;
using System.Reflection;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.Matching.MatchMethods;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services;

public partial class MatchService : EndpointService
{
    private readonly List<IMatchMethod> _matchMethods = new();

    private readonly List<GameRoom> _rooms = new();
    
    public IEnumerable<GameRoom> Rooms => this._rooms.AsReadOnly();

    public int TotalPlayers => this._rooms.SelectMany(r => r.PlayerIds).Count();
    public int TotalPlayersInPod => this._rooms
        .Where(r => r.LevelType == RoomSlotType.Pod)
        .SelectMany(r => r.PlayerIds)
        .Count();

    public MatchService(LoggerContainer<BunkumContext> logger) : base(logger)
    {}

    public GameRoom GetOrCreateRoomByPlayer(GameUser player)
    {
        GameRoom? room = this.GetRoomByPlayer(player);

        // ReSharper disable once InvertIf (happy path goes last)
        if (room == null)
        {
            room = new GameRoom(player);
            this._rooms.Add(room);
        }

        return room;
    }

    public GameRoom? GetRoomByPlayer(GameUser player) 
        => this._rooms.FirstOrDefault(r => r.PlayerIds.Select(s => s.Id).Contains(player.UserId));
    
    public void AddPlayerToRoom(GameUser player, GameRoom targetRoom)
    {
        GameRoom? playersRoom = this.GetRoomByPlayer(player);
        if (playersRoom == null) return; // TODO: error?
        if (targetRoom == playersRoom) return;
        
        foreach (GameRoom room in this._rooms) room.PlayerIds.RemoveAll(i => i.Id == player.UserId);
        targetRoom.PlayerIds.Add(new GameRoomPlayer(player.Username, player.UserId));
    }

    public void AddPlayerToRoom(string username, GameRoom targetRoom)
    {
        foreach (GameRoom room in this._rooms) room.PlayerIds.RemoveAll(i => i.Username == username);
        targetRoom.PlayerIds.Add(new GameRoomPlayer(username, null));
    }

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
        if (method == null) return BadRequest;

        return method.Execute(this, this.Logger, database, user, roomData);
    }
}