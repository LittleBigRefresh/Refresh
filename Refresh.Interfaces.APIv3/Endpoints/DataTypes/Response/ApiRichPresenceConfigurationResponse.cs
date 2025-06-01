using Refresh.Core.Types.Data;
using Refresh.Core.Types.RichPresence;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiRichPresenceConfigurationResponse : IApiResponse, IDataConvertableFrom<ApiRichPresenceConfigurationResponse, RichPresenceConfiguration>
{
    public required string ApplicationId { get; set; }
    public required string PartyIdPrefix { get; set; }
    public required RichPresenceAssetConfiguration AssetConfiguration { get; set; }
    
    public static ApiRichPresenceConfigurationResponse? FromOld(RichPresenceConfiguration? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiRichPresenceConfigurationResponse
        {
            ApplicationId = old.ApplicationId,
            AssetConfiguration = old.Assets,
            PartyIdPrefix = old.PartyIdPrefix,
        };
    }

    public static IEnumerable<ApiRichPresenceConfigurationResponse> FromOldList(IEnumerable<RichPresenceConfiguration> oldList, DataContext dataContext)
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}