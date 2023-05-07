using Bunkum.HttpServer.Responses;
using JetBrains.Annotations;

namespace Refresh.GameServer.Types.Matching;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IMatchMethod
{
    Response Execute(string body);
}