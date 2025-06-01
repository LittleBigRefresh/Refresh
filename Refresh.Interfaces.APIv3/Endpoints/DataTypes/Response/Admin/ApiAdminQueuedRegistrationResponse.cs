using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Admin;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAdminQueuedRegistrationResponse : IApiResponse, IDataConvertableFrom<ApiAdminQueuedRegistrationResponse, QueuedRegistration>
{
    public required string RegistrationId { get; set; }
    public required string Username { get; set; }
    public required string EmailAddress { get; set; }
    public required DateTimeOffset ExpiryDate { get; set; }
    
    public static ApiAdminQueuedRegistrationResponse? FromOld(QueuedRegistration? old, DataContext dataContext)
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

    public static IEnumerable<ApiAdminQueuedRegistrationResponse> FromOldList(IEnumerable<QueuedRegistration> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}