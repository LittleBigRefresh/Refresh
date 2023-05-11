using MongoDB.Bson;

namespace Refresh.GameServer.Types.Matching;

public record GameRoomPlayer(string Username, ObjectId? Id);