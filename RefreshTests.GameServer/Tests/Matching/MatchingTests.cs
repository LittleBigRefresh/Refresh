using System.Diagnostics;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer.Tests.Matching;

public class MatchingTests : GameServerTest
{
    [Test]
    public void CreatesRooms()
    {
        using TestContext context = this.GetServer(false);
        MatchService match = new(Logger);
        match.Initialize();

        SerializedRoomData roomData = new();

        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();

        Token token1 = context.CreateToken(user1);
        Token token2 = context.CreateToken(user2);

        match.ExecuteMethod("UpdateMyPlayerData", roomData, context.Database, user1, token1);
        match.ExecuteMethod("UpdateMyPlayerData", roomData, context.Database, user2, token2);
        
        Assert.Multiple(() =>
        {
            GameRoom? room1;
            GameRoom? room2;
            
            Assert.That(room1 = match.GetRoomByPlayer(user1), Is.Not.Null);
            Assert.That(room2 = match.GetRoomByPlayer(user2), Is.Not.Null);
            
            Assert.That(match.GetRoomByPlayer(user1, token1.TokenPlatform, token1.TokenGame), Is.Not.Null);
            Assert.That(match.GetRoomByPlayer(user2, token2.TokenPlatform, token2.TokenGame), Is.Not.Null);
            
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
        
        SerializedRoomData roomData = new();
        
        // Setup room
        GameUser user1 = context.CreateUser();
        Token token1 = context.CreateToken(user1);
        match.ExecuteMethod("UpdateMyPlayerData", roomData, context.Database, user1, token1);

        // Tell user1 to try to find a room
        Response response = match.ExecuteMethod("FindBestRoom", new SerializedRoomData(), context.Database, user1, token1);
        Assert.That(response.StatusCode, Is.EqualTo(NotFound));
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
        };
        
        // Setup rooms
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        
        Token token1 = context.CreateToken(user1);
        Token token2 = context.CreateToken(user2);
        
        match.ExecuteMethod("UpdateMyPlayerData", roomData, context.Database, user1, token1);
        match.ExecuteMethod("UpdateMyPlayerData", roomData, context.Database, user2, token2);
        
        // Tell user2 to try to find a room
        Response response = match.ExecuteMethod("FindBestRoom", new SerializedRoomData(), context.Database, user2, token2);
        // File.WriteAllBytes("/tmp/matchresp.json", response.Data);
        Assert.That(response.StatusCode, Is.EqualTo(OK));
    }
}