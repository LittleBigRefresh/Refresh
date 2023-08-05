using Refresh.GameServer.Types.Roles;

namespace Refresh.GameServer.Endpoints;

[AttributeUsage(AttributeTargets.Method)]
public class MinimumRoleAttribute : Attribute // TODO: DocAttribute
{
    public GameUserRole MinimumRole { get; init; }

    public MinimumRoleAttribute(GameUserRole minimumRole)
    {
        this.MinimumRole = minimumRole;
    }
}