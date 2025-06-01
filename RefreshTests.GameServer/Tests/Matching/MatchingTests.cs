using System.Diagnostics;
using System.Text;
using Bunkum.Core.Responses;
using Newtonsoft.Json;
using Refresh.Core.Services;
using Refresh.Core.Types.Data;
using Refresh.Core.Types.Matching;
using Refresh.Core.Types.Matching.Responses;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Matching;

public class MatchingTests : GameServerTest
{
    [Test]
    public void CreatesRooms()
    {
        using TestContext context = this.GetServer(false);
        MatchService match = new(Logger);
        match.Initialize();

        SerializedRoomData roomData = new()
        {
            NatType = new List<NatType>
            {
                NatType.Open,
            },
        };

        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();

        Token token1 = context.CreateToken(user1);
        Token token2 = context.CreateToken(user2);

        match.ExecuteMethod("CreateRoom", roomData, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!, //this isn't accessed by matching
            Match = match,
            GuidChecker = null!,
            Token = token1,
        }, context.Server.Value.GameServerConfig);
        match.ExecuteMethod("CreateRoom", roomData, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!,
            Match = match,
            Token = token2,
            GuidChecker = null!,
        }, context.Server.Value.GameServerConfig);
        
        Assert.Multiple(() =>
        {
            GameRoom? room1;
            GameRoom? room2;
            
            Assert.That(room1 = match.RoomAccessor.GetRoomByUser(user1), Is.Not.Null);
            Assert.That(room2 = match.RoomAccessor.GetRoomByUser(user2), Is.Not.Null);
            
            Assert.That(match.RoomAccessor.GetRoomByUser(user1, token1.TokenPlatform, token1.TokenGame), Is.Not.Null);
            Assert.That(match.RoomAccessor.GetRoomByUser(user2, token2.TokenPlatform, token2.TokenGame), Is.Not.Null);
            
            Debug.Assert(room1 != null);
            Debug.Assert(room2 != null);
            
            Assert.That(room1.PlayerIds.Select(i => i.Id), Does.Not.Contain(user2.UserId));
            Assert.That(room2.PlayerIds.Select(i => i.Id), Does.Not.Contain(user1.UserId));
        });
    }
    
    [Test]
    public void DoesntMatchIfNoRooms()
    {
        using TestContext context = this.GetServer(false);
        MatchService match = new(Logger);
        match.Initialize();
        
        SerializedRoomData roomData = new()
        {
            NatType = new List<NatType>
            {
                NatType.Open,
            },
        };
        
        // Setup room
        GameUser user1 = context.CreateUser();
        Token token1 = context.CreateToken(user1);
        match.ExecuteMethod("CreateRoom", roomData, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            GuidChecker = null!,
            DataStore = null!, //this isn't accessed by matching
            Match = match,
            Token = token1,
        }, context.Server.Value.GameServerConfig);
        
        // Tell user1 to try to find a room
        Response response = match.ExecuteMethod("FindBestRoom", new SerializedRoomData
        {
            NatType = new List<NatType>
            {
                NatType.Open,
            },
        }, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            GuidChecker = null!,
            DataStore = null!, //this isn't accessed by matching
            Match = match,
            Token = token1,
        }, context.Server.Value.GameServerConfig);

        // Deserialize the result
        List<SerializedStatusCodeMatchResponse> responseObjects =
            JsonConvert.DeserializeObject<List<SerializedStatusCodeMatchResponse>>(Encoding.UTF8.GetString(response.Data))!;

        //Make sure the only result is a 404 object
        Assert.That(responseObjects, Has.Count.EqualTo(1));
        Assert.That(response.StatusCode, Is.EqualTo(NotFound));
        Assert.That(responseObjects[0].StatusCode, Is.EqualTo(NotFound));
    }

    [Test]
    public void StrictNatCantJoinStrict()
    {
        using TestContext context = this.GetServer(false);
        MatchService match = new(Logger);
        match.Initialize();
        
        SerializedRoomData roomData = new()
        {
            Mood = (byte)RoomMood.AllowingAll, // Tells their rooms that they can be matched with each other
            NatType = new List<NatType>
            {
                NatType.Strict,
            },
        };
        
        // Setup rooms
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        
        Token token1 = context.CreateToken(user1);
        Token token2 = context.CreateToken(user2);
        
        match.ExecuteMethod("CreateRoom", roomData, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!, //this isn't accessed by matching
            Match = match,
            GuidChecker = null!,
            Token = token1,
        }, context.Server.Value.GameServerConfig);
        match.ExecuteMethod("CreateRoom", roomData, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!,
            Match = match,
            Token = token2,
            GuidChecker = null!,
        }, context.Server.Value.GameServerConfig);
        
        // Tell user2 to try to find a room
        Response response = match.ExecuteMethod("FindBestRoom", new SerializedRoomData { NatType = [NatType.Strict], }, new DataContext {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!,
            Match = match,
            GuidChecker = null!,
            Token = token2,
        }, context.Server.Value.GameServerConfig);
        
        //Deserialize the result
        List<SerializedStatusCodeMatchResponse> responseObjects =
            JsonConvert.DeserializeObject<List<SerializedStatusCodeMatchResponse>>(Encoding.UTF8.GetString(response.Data))!;

        //Make sure the only result is a 404 object
        Assert.That(responseObjects, Has.Count.EqualTo(1));
        Assert.That(response.StatusCode, Is.EqualTo(NotFound));
        Assert.That(responseObjects[0].StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void StrictNatCanJoinOpen()
    {
        using TestContext context = this.GetServer(false);
        MatchService match = new(Logger);
        match.Initialize();
        
        SerializedRoomData roomData = new()
        {
            Mood = (byte)RoomMood.AllowingAll, // Tells their rooms that they can be matched with each other
            NatType = new List<NatType>
            {
                NatType.Open,
            },
        };
        
        SerializedRoomData roomData2 = new()
        {
            Mood = (byte)RoomMood.AllowingAll, // Tells their rooms that they can be matched with each other
            NatType = new List<NatType>
            {
                NatType.Strict,
            },
        };
        
        // Setup rooms
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        
        Token token1 = context.CreateToken(user1);
        Token token2 = context.CreateToken(user2);
        
        match.ExecuteMethod("CreateRoom", roomData, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!, //this isn't accessed by matching
            Match = match,
            Token = token1,
            GuidChecker = null!,
        }, context.Server.Value.GameServerConfig);
        match.ExecuteMethod("CreateRoom", roomData2, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!,
            Match = match,
            Token = token2, 
            GuidChecker = null!,
        }, context.Server.Value.GameServerConfig);
        
        // Tell user2 to try to find a room
        Response response = match.ExecuteMethod("FindBestRoom", new SerializedRoomData
        {
            NatType = new List<NatType> {
                NatType.Strict,
            },
        }, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!, //this isn't accessed by matching
            Match = match,
            Token = token2,
            GuidChecker = null!,
        }, context.Server.Value.GameServerConfig);
        Assert.That(response.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    public void MatchesPlayersTogether()
    {
        using TestContext context = this.GetServer(false);
        MatchService match = new(Logger);
        match.Initialize();
        
        SerializedRoomData roomData = new()
        {
            Mood = (byte)RoomMood.AllowingAll, // Tells their rooms that they can be matched with each other
            NatType = new List<NatType>
            {
                NatType.Open,
            },
        };
        
        // Setup rooms
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        
        Token token1 = context.CreateToken(user1);
        Token token2 = context.CreateToken(user2);
        
        match.ExecuteMethod("CreateRoom", roomData, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!, //this isn't accessed by matching
            Match = match,
            Token = token1,
            GuidChecker = null!,
        }, context.Server.Value.GameServerConfig);
        match.ExecuteMethod("CreateRoom", roomData, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!,
            Match = match,
            Token = token2,
            GuidChecker = null!,
        }, context.Server.Value.GameServerConfig);
        
        // Tell user2 to try to find a room
        Response response = match.ExecuteMethod("FindBestRoom", new SerializedRoomData
        {
            NatType = new List<NatType> {
                NatType.Open,
            },
        }, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!, //this isn't accessed by matching
            Match = match,
            Token = token2,
            GuidChecker = null!,
        }, context.Server.Value.GameServerConfig);
        Assert.That(response.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    public void HostCanSetPlayersInRoom()
    {
        using TestContext context = this.GetServer(false);
        MatchService match = new(Logger);
        match.Initialize();
        
        SerializedRoomData roomData = new()
        {
            Mood = (byte)RoomMood.AllowingAll, // Tells their rooms that they can be matched with each other
            NatType = new List<NatType>
            {
                NatType.Open,
            },
        };
        
        // Setup rooms
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        
        Token token1 = context.CreateToken(user1);
        Token token2 = context.CreateToken(user2);
        
        match.ExecuteMethod("CreateRoom", roomData, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!, //this isn't accessed by matching
            Match = match,
            Token = token1,
            GuidChecker = null!,
        }, context.Server.Value.GameServerConfig);
        match.ExecuteMethod("CreateRoom", roomData, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!,
            Match = match,
            Token = token2,
            GuidChecker = null!,
        }, context.Server.Value.GameServerConfig);
        
        // Get user1 and user2 in the same room
        roomData.Players = new List<string>
        {
            user1.Username,
            user2.Username,
        };

        match.ExecuteMethod("UpdatePlayersInRoom", roomData, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!, //this isn't accessed by matching
            Match = match,
            Token = token1,
            GuidChecker = null!,
        }, context.Server.Value.GameServerConfig);
        GameRoom? room = match.RoomAccessor.GetRoomByUser(user1);
        Assert.Multiple(() =>
        {
            Assert.That(room, Is.Not.Null);
            Assert.That(room!.PlayerIds, Has.Count.EqualTo(2));
            Assert.That(room.PlayerIds.FirstOrDefault(r => r.Id == user1.UserId), Is.Not.Null);
            Assert.That(room.PlayerIds.FirstOrDefault(r => r.Id == user2.UserId), Is.Not.Null);
        });
    }
    
    [Test]
    public void PlayersCanLeaveAndSplitIntoNewRoom()
    {
        using TestContext context = this.GetServer(false);
        MatchService match = new(Logger);
        match.Initialize();
        
        SerializedRoomData roomData = new()
        {
            Mood = (byte)RoomMood.AllowingAll, // Tells their rooms that they can be matched with each other
            NatType = new List<NatType>
            {
                NatType.Open,
            },
        };
        
        // Setup rooms
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        
        Token token1 = context.CreateToken(user1);
        Token token2 = context.CreateToken(user2);
        
        match.ExecuteMethod("CreateRoom", roomData, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!, //this isn't accessed by matching
            Match = match,
            Token = token1,
            GuidChecker = null!,
        }, context.Server.Value.GameServerConfig);
        match.ExecuteMethod("CreateRoom", roomData, new DataContext
        {
            Database = context.Database,
            Logger = context.Server.Value.Logger,
            DataStore = null!,
            Match = match,
            Token = token2,
            GuidChecker = null!,
        }, context.Server.Value.GameServerConfig);
        
        // Get user1 and user2 in the same room
        roomData.Players = new List<string>
        {
            user1.Username,
            user2.Username,
        };

        {
            match.ExecuteMethod("UpdatePlayersInRoom", roomData, new DataContext
            {
                Database = context.Database,
                Logger = context.Server.Value.Logger,
                DataStore = null!, //this isn't accessed by matching
                Match = match,
                Token = token1,
                GuidChecker = null!,
            }, context.Server.Value.GameServerConfig);
            GameRoom? user1Room = match.RoomAccessor.GetRoomByUser(user1);
            Assert.That(user1Room, Is.Not.Null);
            Assert.That(user1Room!.PlayerIds.FirstOrDefault(r => r.Id == user2.UserId), Is.Not.Null);
        }

        {
            match.ExecuteMethod("CreateRoom", roomData, new DataContext
            {
                Database = context.Database,
                Logger = context.Server.Value.Logger,
                DataStore = null!, //this isn't accessed by matching
                Match = match,
                Token = token2,
                GuidChecker = null!,
            }, context.Server.Value.GameServerConfig);
            GameRoom? user1Room = match.RoomAccessor.GetRoomByUser(user1);
            GameRoom? user2Room = match.RoomAccessor.GetRoomByUser(user2);
            Assert.That(user1Room, Is.Not.Null);
            Assert.That(user2Room, Is.Not.Null);
            Assert.That(user1Room!.PlayerIds.FirstOrDefault(r => r.Id == user2.UserId), Is.Null);
            Assert.That(user2Room!.PlayerIds.First().Id, Is.EqualTo(user2.UserId));
        }

    }
    
    [Test]
    public void DoesntMatchIfLookingForLevelWhenPod()
    {
        using TestContext context = this.GetServer(false);
        MatchService match = new(Logger);
        match.Initialize();
        
        SerializedRoomData roomData = new()
        {
            Mood = (byte)RoomMood.AllowingAll, // Tells their rooms that they can be matched with each other
            NatType = new List<NatType>
            {
                NatType.Open,
            },
            _Slot =
            [
                (int)RoomSlotType.Pod,
                0,
            ],
        };
        
        // Setup rooms
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        
        Token token1 = context.CreateToken(user1);
        Token token2 = context.CreateToken(user2);

        match.ExecuteMethod("CreateRoom", roomData, context.GetDataContext(token1), context.Server.Value.GameServerConfig);
        match.ExecuteMethod("CreateRoom", roomData, context.GetDataContext(token2), context.Server.Value.GameServerConfig);
        
        // Tell user2 to try to find a room
        Response response = match.ExecuteMethod("FindBestRoom", new SerializedRoomData
        {
            NatType = new List<NatType> {
                NatType.Open,
            },
            _Slot =
            [
                (int)RoomSlotType.Online,
                1337,
            ],
        }, context.GetDataContext(token2), context.Server.Value.GameServerConfig);
        Assert.That(response.StatusCode, Is.EqualTo(NotFound));
    }
}