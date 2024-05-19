using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.RichPresence;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

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

    public static IEnumerable<ApiRichPresenceConfigurationResponse> FromOldList(
        IEnumerable<RichPresenceConfiguration> oldList, DataContext dataContext)
    {
        throw new NotImplementedException();
    }
}