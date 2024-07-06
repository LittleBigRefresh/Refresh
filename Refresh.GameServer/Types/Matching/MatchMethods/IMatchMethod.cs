using Bunkum.Core.Responses;
using JetBrains.Annotations;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Types.Data;

namespace Refresh.GameServer.Types.Matching.MatchMethods;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IMatchMethod
{
    IEnumerable<string> MethodNames { get; }

    Response Execute(DataContext dataContext, SerializedRoomData body, GameServerConfig gameServerConfig);
}