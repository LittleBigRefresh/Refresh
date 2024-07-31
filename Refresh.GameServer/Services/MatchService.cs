using System.Collections.Frozen;
using Bunkum.Core;
using Bunkum.Core.Services;
using NotEnoughLogs;
using System.Reflection;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.Matching.MatchMethods;
using Refresh.GameServer.Types.Matching.Responses;
using Refresh.GameServer.Types.Matching.RoomAccessors;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services;

public partial class MatchService(Logger logger) : EndpointService(logger)
{
    private FrozenSet<IMatchMethod> _matchMethods = null!; // initialized in Initialize()

    public IRoomAccessor RoomAccessor { get; private set; } = null!; //initialized in Initialize()
    
    public GameRoom GetOrCreateRoomByPlayer(
        GameUser player, 
        TokenPlatform platform, 
        TokenGame game, 
        NatType natType,
        bool? passedNoJoinPoint = null)
    {
        GameRoom? room = this.RoomAccessor.GetRoomByUser(player, platform, game);
        
        // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
        if (room == null)
            room = this.CreateRoomByPlayer(player, platform, game, natType, passedNoJoinPoint);

        return room;
    }

    public GameRoom CreateRoomByPlayer(
        GameUser player, 
        TokenPlatform platform, 
        TokenGame game, 
        NatType natType,
        bool? passedNoJoinPoint = null)
    {
        GameRoom room = new(player, platform, game, natType, passedNoJoinPoint);
        this.RoomAccessor.AddRoom(room);
        return room;
    }

    public GameRoom SplitUserIntoNewRoom(
        GameUser player, 
        TokenPlatform platform, 
        TokenGame game, 
        NatType natType,
        bool? passedNoJoinPoint = null)
    {
        GameRoom? room = this.RoomAccessor.GetRoomByUser(player, platform, game);
        if (room == null)
        {
            return this.CreateRoomByPlayer(player, platform, game, natType, passedNoJoinPoint);
        }
        
        // Remove player from old room
        room.PlayerIds.RemoveAll(i => i.Id == player.UserId);
        // Update the room on the room accessor
        this.RoomAccessor.UpdateRoom(room);
        
        return this.CreateRoomByPlayer(player, platform, game, natType, passedNoJoinPoint);
    }
    
    public int GetPlayerCountForLevel(RoomSlotType type, int id)
    {
        return this.RoomAccessor.GetRoomsInLevel(type, id).Sum(r => r.PlayerIds.Count);
    }

    public void AddPlayerToRoom(GameUser player, GameRoom targetRoom, TokenPlatform platform, TokenGame game)
    {
        GameRoom? playersRoom = this.RoomAccessor.GetRoomByUser(player, platform, game);
        if (playersRoom == null) return; // TODO: error?
        if (targetRoom == playersRoom) return;

        foreach (GameRoom room in this.RoomAccessor.GetAllRooms())
        {
            int removed = room.PlayerIds.RemoveAll(i => i.Id == player.UserId);
            if(removed > 0) this.RoomAccessor.UpdateRoom(room);
        }
        targetRoom.PlayerIds.Add(new GameRoomPlayer(player.Username, player.UserId));
        this.RoomAccessor.UpdateRoom(targetRoom);
    }

    public void AddPlayerToRoom(string username, GameRoom targetRoom)
    {
        foreach (GameRoom room in this.RoomAccessor.GetAllRooms())
        {
            int removed = room.PlayerIds.RemoveAll(i => i.Username == username);
            if(removed > 0) this.RoomAccessor.UpdateRoom(room);
        }
        targetRoom.PlayerIds.Add(new GameRoomPlayer(username, null));
        this.RoomAccessor.UpdateRoom(targetRoom);
    }

    public override void Initialize()
    {
        // TODO: discover match methods via source generation
        List<Type> matchMethodTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IMatchMethod)) && t != typeof(IMatchMethod))
            .ToList();

        List<IMatchMethod> matchMethods = new(matchMethodTypes.Count);

        foreach (Type type in matchMethodTypes)
        {
            string name = type.Name.Substring(0, type.Name.IndexOf("Method", StringComparison.Ordinal));
            this.Logger.LogTrace(BunkumCategory.Service, "Found {0} '{1}'", nameof(IMatchMethod), name);
            
            matchMethods.Add((IMatchMethod)Activator.CreateInstance(type)!);
        }

        this._matchMethods = matchMethods.ToFrozenSet();
        this.Logger.LogDebug(BunkumCategory.Service, "Discovered {0} match method types", this._matchMethods.Count);

        this.RoomAccessor = new InMemoryRoomAccessor(this.Logger);
    }

    private IMatchMethod? TryGetMatchMethod(string method) 
        => this._matchMethods.FirstOrDefault(m => m.MethodNames.Contains(method));

    public Response ExecuteMethod(string methodStr, SerializedRoomData roomData, DataContext dataContext, GameServerConfig gameServerConfig)
    {
        IMatchMethod? method = this.TryGetMatchMethod(methodStr);
        if (method == null) return BadRequest;
        
        Response response = method.Execute(dataContext, roomData, gameServerConfig);
        
        // If there's a response data specified, then there's nothing more we need to do
        if (response.Data.Length != 0) 
            return response;
        
        // If there's no response body, then we need to make our own using the status code
        SerializedStatusCodeMatchResponse status = new(response.StatusCode);
        
        List<object> responseData = [status];
        
        response = new Response(responseData, ContentType.Json, response.StatusCode);
        
        return response;
    }
}