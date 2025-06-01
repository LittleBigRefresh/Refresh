using Bunkum.Core.Responses;
using JetBrains.Annotations;
using Refresh.Core.Configuration;
using Refresh.Core.Types.Data;

namespace Refresh.Core.Types.Matching.MatchMethods;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IMatchMethod
{
    IEnumerable<string> MethodNames { get; }

    Response Execute(DataContext dataContext, SerializedRoomData body, GameServerConfig gameServerConfig);
}