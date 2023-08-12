using Refresh.GameServer.Configuration;
using Refresh.GameServer.Types.RichPresence;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiRichPresenceConfigurationResponse : IApiResponse, IDataConvertableFrom<ApiRichPresenceConfigurationResponse, RichPresenceConfiguration>
{
    public required string ApplicationId { get; set; }
    public required string PartyIdPrefix { get; set; }
    public required RichPresenceAssetConfiguration AssetConfiguration { get; set; }
    
    public static ApiRichPresenceConfigurationResponse? FromOld(RichPresenceConfiguration? old)
    {
        if (old == null) return null;

        return new ApiRichPresenceConfigurationResponse
        {
            ApplicationId = old.ApplicationId.ToString(),
            AssetConfiguration = old.Assets,
            PartyIdPrefix = old.PartyIdPrefix,
        };
    }

    public static IEnumerable<ApiRichPresenceConfigurationResponse> FromOldList(IEnumerable<RichPresenceConfiguration> oldList)
    {
        throw new NotImplementedException();
    }
}