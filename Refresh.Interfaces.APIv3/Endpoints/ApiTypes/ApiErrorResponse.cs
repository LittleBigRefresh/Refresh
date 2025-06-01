using AttribDoc;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes;

namespace Refresh.Interfaces.APIv3.Endpoints.ApiTypes;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiErrorResponse : IApiResponse
{
    public required string Name { get; set; }
    public required string OccursWhen { get; set; }
    
    public static ApiErrorResponse? FromOld(Error? old)
    {
        if (old == null) return null;

        return new ApiErrorResponse
        {
            Name = old.Name,
            OccursWhen = old.OccursWhen,
        };
    }

    public static IEnumerable<ApiErrorResponse> FromOldList(IEnumerable<Error> oldList) => oldList.Select(old => FromOld(old)).ToList()!;
}