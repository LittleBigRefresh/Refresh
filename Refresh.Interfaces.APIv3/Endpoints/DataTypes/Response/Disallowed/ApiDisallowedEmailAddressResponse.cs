using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Disallowed;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiDisallowedEmailAddressResponse : IApiResponse, IDataConvertableFrom<ApiDisallowedEmailAddressResponse, DisallowedEmailAddress>
{
    public required string AddressLower { get; set; }
    public required string Reason { get; set; }
    public required DateTimeOffset DisallowedAt { get; set; }
    
    public static ApiDisallowedEmailAddressResponse? FromOld(DisallowedEmailAddress? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiDisallowedEmailAddressResponse
        {
            AddressLower = old.AddressLower,
            Reason = old.Reason,
            DisallowedAt = old.DisallowedAt,
        };
    }

    public static IEnumerable<ApiDisallowedEmailAddressResponse> FromOldList(IEnumerable<DisallowedEmailAddress> oldList, DataContext dataContext)
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}