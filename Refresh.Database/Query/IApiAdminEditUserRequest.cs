using Refresh.Database.Models.Users;

namespace Refresh.Database.Query;

public interface IApiAdminEditUserRequest
{
    string? IconHash { get; set; }
    string? VitaIconHash { get; set; }
    string? BetaIconHash { get; set; }
    string? Description { get; set; }
    GameUserRole? Role { get; set; }
}