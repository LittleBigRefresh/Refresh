using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Disallowed;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiDisallowedEmailDomainResponse : IApiResponse, IDataConvertableFrom<ApiDisallowedEmailDomainResponse, DisallowedEmailDomain>
{
    public required string Domain { get; set; }
    public required string Reason { get; set; }
    public required DateTimeOffset DisallowedAt { get; set; }
    
    public static ApiDisallowedEmailDomainResponse? FromOld(DisallowedEmailDomain? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiDisallowedEmailDomainResponse
        {
            Domain = old.Domain,
            Reason = old.Reason,
            DisallowedAt = old.DisallowedAt,
        };
    }

    public static IEnumerable<ApiDisallowedEmailDomainResponse> FromOldList(IEnumerable<DisallowedEmailDomain> oldList, DataContext dataContext)
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}