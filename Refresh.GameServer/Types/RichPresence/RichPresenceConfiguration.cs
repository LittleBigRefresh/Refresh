using Refresh.GameServer.Configuration;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;

namespace Refresh.GameServer.Types.RichPresence;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RichPresenceConfiguration : IApiResponse
{
    public required long ApplicationId { get; set; }
    public required string PartyIdPrefix { get; set; }
    public required RichPresenceUsernameResponseType UsernameType { get; set; }
    public required RichPresenceAssetConfiguration AssetConfiguration { get; set; }

    public static RichPresenceConfiguration Create(GameServerConfig gameConfig, RichPresenceConfig richConfig, bool legacy = false)
    {
        return new RichPresenceConfiguration
        {
            ApplicationId = richConfig.ApplicationId,
            AssetConfiguration = new RichPresenceAssetConfiguration
            {
                PodAsset = richConfig.PodAsset,
                MoonAsset = richConfig.PodAsset,
                RemoteMoonAsset = richConfig.PodAsset,
                DeveloperAsset = richConfig.PodAsset,
                DeveloperAdventureAsset = richConfig.PodAsset,
                DlcAsset = richConfig.PodAsset,
                FallbackAsset = richConfig.PodAsset,
            },
            UsernameType = legacy ? RichPresenceUsernameResponseType.Username : RichPresenceUsernameResponseType.UserId,
            PartyIdPrefix = gameConfig.InstanceName.Replace(' ', '_'),
        };
    }
}