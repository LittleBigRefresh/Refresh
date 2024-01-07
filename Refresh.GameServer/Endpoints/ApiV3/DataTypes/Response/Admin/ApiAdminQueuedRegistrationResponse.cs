using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Admin;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAdminQueuedRegistrationResponse : IApiResponse, IDataConvertableFrom<ApiAdminQueuedRegistrationResponse, QueuedRegistration>
{
    public required string RegistrationId { get; set; }
    public required string Username { get; set; }
    public required string EmailAddress { get; set; }
    public required DateTimeOffset ExpiryDate { get; set; }
    
    public static ApiAdminQueuedRegistrationResponse? FromOld(QueuedRegistration? old)
    {
        if (old == null) return null;

        return new ApiAdminQueuedRegistrationResponse
        {
            Username = old.Username,
            EmailAddress = old.EmailAddress,
            ExpiryDate = old.ExpiryDate,
            RegistrationId = old.RegistrationId.ToString()!,
        };
    }

    public static IEnumerable<ApiAdminQueuedRegistrationResponse> FromOldList(IEnumerable<QueuedRegistration> oldList) => oldList.Select(FromOld).ToList()!;
}