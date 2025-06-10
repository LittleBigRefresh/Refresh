using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiUpdateUserRequest : IApiEditUserRequest
{
    public string? IconHash { get; set; }
    public string? Description { get; set; }
    public bool? AllowIpAuthentication { get; set; }
    
    public bool? PsnAuthenticationAllowed { get; set; }
    public bool? RpcnAuthenticationAllowed { get; set; }

    public bool? UnescapeXmlSequences { get; set; }
    
    public string? EmailAddress { get; set; }
    
    public bool? ShowModdedContent { get; set; }
    public bool? ShowReuploadedContent { get; set; }

    public Visibility? LevelVisibility { get; set; }
    public Visibility? ProfileVisibility { get; set; }
}