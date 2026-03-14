using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Core.Configuration;
using Refresh.Core.Services;
using Refresh.Core.Types.Matching;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users.Rooms;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class MatchingApiTests : GameServerTest
{
    [Test]
    public void GetsRoomByUserUuidAndName()
    {
        using TestContext context = this.GetServer();
        GameUser player = context.CreateUser();
        GameServerConfig config = context.Server.Value.GameServerConfig;
        config.PermitShowingOnlineUsers = true;

        MatchService match = context.GetService<MatchService>();
        match.Initialize();
        GameRoom room = match.CreateRoomByPlayer(player, TokenPlatform.PS3, TokenGame.LittleBigPlanet2, NatType.Open);
        
        // UUID
        ApiResponse<ApiGameRoomResponse>? response = context.Http.GetData<ApiGameRoomResponse>($"/api/v3/rooms/uuid/{player.UserId}");
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.RoomId.ToString(), Is.EqualTo(room.RoomId.ToString()));

        // name
        response = context.Http.GetData<ApiGameRoomResponse>($"/api/v3/rooms/name/{player.Username}");
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.RoomId.ToString(), Is.EqualTo(room.RoomId.ToString()));
    }
}