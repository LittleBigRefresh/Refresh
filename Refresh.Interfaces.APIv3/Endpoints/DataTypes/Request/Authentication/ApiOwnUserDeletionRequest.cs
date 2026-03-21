#nullable disable
namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request.Authentication;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiOwnUserDeletionRequest
{
    public string PasswordSha512 { get; set; }
}