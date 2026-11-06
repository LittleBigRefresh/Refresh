namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAdminUpdateEmailAddressRequest : ApiAdminRequest
{
    public string? EmailAddress { get; set; }
    public bool? MarkEmailAddressAsVerified { get; set; }
    public bool? DisallowOldEmailAddress { get; set; }
    // Probably shouldn't allow to disallow an entire domain from here
}