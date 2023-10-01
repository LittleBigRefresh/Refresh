using Bunkum.Core;
using Bunkum.Core.Services;
using NotEnoughLogs;
using System.Reflection;
using Bunkum.Core.Responses;
using MongoDB.Bson;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.Matching.MatchMethods;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services;

public partial class MatchService : EndpointService
{
    private readonly List<IMatchMethod> _matchMethods = new();

    private readonly List<GameRoom> _rooms = new();

    private readonly Dictionary<ObjectId, ObjectId> _forceMatches = new();
    
    public IEnumerable<GameRoom> Rooms
    {
        get
        {
            this.RemoveExpiredRooms();
            return this._rooms.AsReadOnly();
        }
    }

    public int TotalPlayers => this._rooms.SelectMany(r => r.PlayerIds).Count();
    public int TotalPlayersInPod => this._rooms
        .Where(r => r.LevelType == RoomSlotType.Pod)
        .SelectMany(r => r.PlayerIds)
        .Count();

    public MatchService(Logger logger) : base(logger)
    {}

    public GameRoom GetOrCreateRoomByPlayer(GameUser player, TokenPlatform platform, TokenGame game, NatType natType)
    {
        GameRoom? room = this.GetRoomByPlayer(player, platform, game);

        // ReSharper disable once InvertIf (happy path goes last)
        if (room == null)
        {
            room = new GameRoom(player, platform, game, natType);
            this._rooms.Add(room);
        }

        return room;
    }
    
    public GameRoom? GetRoomByPlayer(GameUser player)
    {
        this.RemoveExpiredRooms();
        return this._rooms.FirstOrDefault(r => r.PlayerIds.Select(s => s.Id).Contains(player.UserId));
    }

    public GameRoom? GetRoomByPlayer(GameUser player, TokenPlatform platform, TokenGame game)
    {
        this.RemoveExpiredRooms();
        return this._rooms.FirstOrDefault(r => r.PlayerIds.Select(s => s.Id).Contains(player.UserId) &&
                                               r.Platform == platform &&
                                               r.Game == game);
    }

    public int GetPlayerCountForLevel(RoomSlotType type, int id)
    {
        return this._rooms.Where(r => r.LevelType == type && r.LevelId == id)
            .Sum(r => r.PlayerIds.Count);
    }

    public void AddPlayerToRoom(GameUser player, GameRoom targetRoom, TokenPlatform platform, TokenGame game)
    {
        GameRoom? playersRoom = this.GetRoomByPlayer(player, platform, game);
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

    public void RemoveExpiredRooms()
    {
        int removed = this._rooms.RemoveAll(r => r.IsExpired);
        if (removed == 0) return;
        
        this.Logger.LogDebug(BunkumCategory.Matching, $"Removed {removed} expired rooms");
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
            this.Logger.LogTrace(BunkumCategory.Service, $"Found {nameof(IMatchMethod)} '{name}'");
            
            this._matchMethods.Add((IMatchMethod)Activator.CreateInstance(type)!);
        }
        
        this.Logger.LogDebug(BunkumCategory.Service, $"Discovered {matchMethodTypes.Count} match method types");
    }

    private IMatchMethod? TryGetMatchMethod(string method) 
        => this._matchMethods.FirstOrDefault(m => m.MethodNames.Contains(method));

    public Response ExecuteMethod(string methodStr, SerializedRoomData roomData, GameDatabaseContext database, GameUser user, Token token)
    {
        IMatchMethod? method = this.TryGetMatchMethod(methodStr);
        if (method == null) return BadRequest;

        return method.Execute(this, this.Logger, database, user, token, roomData);
    }

    public void SetForceMatch(ObjectId user, ObjectId target)
    {
        this._forceMatches[user] = target;
    }

    public ObjectId? GetForceMatch(ObjectId user)
    {
        if (this._forceMatches.TryGetValue(user, out ObjectId target))
        {
            return target;
        }

        return null;
    }
    
    public void ClearForceMatch(ObjectId user)
    {
        this._forceMatches.Remove(user);
    }
}