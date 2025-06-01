using MongoDB.Bson;

namespace Refresh.Core.Types.Matching;

public record GameRoomPlayer(string Username, ObjectId? Id);