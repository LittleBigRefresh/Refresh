using Refresh.GameServer.Configuration;

namespace Refresh.GameServer.Types.RichPresence;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RichPresenceConfiguration
{
    public required string ApplicationId { get; set; }
    public required string PartyIdPrefix { get; set; }
    public required RichPresenceUsernameResponseType UsernameType { get; set; }
    public required RichPresenceAssetConfiguration Assets { get; set; }

    public static RichPresenceConfiguration Create(GameServerConfig gameConfig, RichPresenceConfig richConfig, bool legacy = false)
    {
        return new RichPresenceConfiguration
        {
            ApplicationId = richConfig.ApplicationId.ToString(),
            Assets = new RichPresenceAssetConfiguration
            {
                PodAsset = richConfig.PodAsset,
                MoonAsset = richConfig.MoonAsset,
                RemoteMoonAsset = richConfig.RemoteMoonAsset,
                DeveloperAsset = richConfig.DeveloperAsset,
                DeveloperAdventureAsset = richConfig.DeveloperAdventureAsset,
                DlcAsset = richConfig.DlcAsset,
                FallbackAsset = richConfig.FallbackAsset,
            },
            UsernameType = legacy ? RichPresenceUsernameResponseType.Username : RichPresenceUsernameResponseType.UserId,
            PartyIdPrefix = gameConfig.InstanceName.Replace(' ', '_'),
        };
    }
}