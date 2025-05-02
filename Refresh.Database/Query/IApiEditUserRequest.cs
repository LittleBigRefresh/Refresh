using Refresh.GameServer.Types;

namespace Refresh.Database.Query;

public interface IApiEditUserRequest
{
    string? IconHash { get; set; }
    string? Description { get; set; }
    bool? AllowIpAuthentication { get; set; }
    bool? PsnAuthenticationAllowed { get; set; }
    bool? RpcnAuthenticationAllowed { get; set; }
    bool? UnescapeXmlSequences { get; set; }
    string? EmailAddress { get; set; }
    bool? ShowModdedContent { get; set; }
    Visibility? LevelVisibility { get; set; }
    Visibility? ProfileVisibility { get; set; }
}