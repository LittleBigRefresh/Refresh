using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Challenges.Lbp3;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Handshake;

public class MetadataEndpoints : EndpointGroup
{
    [GameEndpoint("privacySettings", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedPrivacySettings GetPrivacySettings(RequestContext context, GameUser user)
    {
        return new SerializedPrivacySettings
        {
            LevelVisibility = user.LevelVisibility,
            ProfileVisibility = user.ProfileVisibility,
        };
    }
    
    [GameEndpoint("privacySettings", ContentType.Xml, HttpMethods.Post)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedPrivacySettings SetPrivacySettings(RequestContext context, SerializedPrivacySettings body, GameDatabaseContext database, GameUser user)
    {
        database.SetPrivacySettings(user, body);
        
        return body;
    }

    [GameEndpoint("npdata", ContentType.Xml, HttpMethods.Post)]
    public Response SetFriendData(RequestContext context, GameUser user, GameDatabaseContext database, SerializedFriendData body)
    {
        IEnumerable<GameUser> friends = body.FriendsList.Names
            .Take(128) // should be way more than enough - we'll see if this becomes a problem
            .Select(username => database.GetUserByUsername(username))
            .Where(u => u != null)!;
        
        foreach (GameUser userToFavourite in friends)
            database.FavouriteUser(userToFavourite, user);
        
        return OK;
    }

    private static readonly Lazy<string?> NetworkSettingsFile
        = new(() => 
        {
            string path = Path.Combine(Environment.CurrentDirectory, "network_settings.nws");

            return File.Exists(path) ? File.ReadAllText(path) : null;
        });
    
    [GameEndpoint("network_settings.nws")]
    [MinimumRole(GameUserRole.Restricted)]
    public string NetworkSettings(RequestContext context, GameServerConfig config)
    {
        bool created = NetworkSettingsFile.IsValueCreated;
        
        string? networkSettings = NetworkSettingsFile.Value;
        
        // Only log this warning once
        if(!created && networkSettings == null)
            context.Logger.LogWarning(BunkumCategory.Request, "network_settings.nws file is missing! " +
                                                              "We've defaulted to one with sane defaults, but it may be relevant to write your own if you are an advanced user. " +
                                                              "If everything works the way you like, you can safely ignore this warning.");

        // EnableHackChecks being false fixes the "There was a problem with the level you were playing on that forced a return to your Pod." error that LBP3 tends to show in the pod.
        // AlexDB
        //  - Enables the "Web Privacy Settings" option on LBP1
        //  - Enables in-game queuing on LBP1
        //  - Part of the check for enabling LBP1 Playlists
        //  - Adds "Mm Picks" and "Lucky Dip" search options on LBP1
        // OverheatingThreshholdDisallowMidgameJoin is set to >1.0 so that it never triggers
        // EnableCommunityDecorations, EnablePlayedFilter, EnableDiveIn enable various game features
        // DisableDLCPublishCheck disables the game's DLC publish check.
        networkSettings ??= $"""
                            AllowOnlineCreate true
                            ShowErrorNumbers true
                            AllowModeratedLevels false
                            AllowModeratedPoppetItems false
                            ShowLevelBoos true
                            CDNHostName {config.GameConfigStorageUrl}
                            TelemetryServer {config.GameConfigStorageUrl}
                            OverheatingThresholdDisallowMidgameJoin 2.0
                            EnableCommunityDecorations true
                            EnablePlayedFilter true
                            EnableDiveIn true
                            EnableHackChecks false
                            DisableDLCPublishCheck true
                            AlexDB true

                            """;
        
        return networkSettings;
    }
    
    private static readonly Lazy<string?> TelemetryConfigFile
        = new(() => 
        {
            string path = Path.Combine(Environment.CurrentDirectory, "telemetry.xml");

            return File.Exists(path) ? File.ReadAllText(path) : null;
        });

    [GameEndpoint("t_conf")]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(Gone)]
    public string? TelemetryConfig(RequestContext context) 
    {
        bool created = TelemetryConfigFile.IsValueCreated;
        
        string? telemetryConfig = TelemetryConfigFile.Value;
        
        // Only log this warning once
        if (!created && telemetryConfig == null)
            context.Logger.LogWarning(BunkumCategory.Request, "telemetry.xml file is missing! " +
                                                             "LBP will work without it, but it may be relevant to you if you are an advanced user.");

        return telemetryConfig;
    }
    
    private static readonly Lazy<string?> PromotionsFile
        = new(() => 
        {
            string path = Path.Combine(Environment.CurrentDirectory, "promotions.xml");

            return File.Exists(path) ? File.ReadAllText(path) : null;
        });

    [GameEndpoint("promotions")]
    [NullStatusCode(OK)]
    [MinimumRole(GameUserRole.Restricted)]
    public string? Promotions(RequestContext context) 
    {
        bool created = PromotionsFile.IsValueCreated;
        
        string? promotions = PromotionsFile.Value;
        
        // Only log this warning once
        if(!created && promotions == null)
            context.Logger.LogWarning(BunkumCategory.Request, "promotions.xml file is missing! " +
                                                             "LBP will work without it, but it may be relevant to you if you are an advanced user.");
        
        return promotions;
    }
    
    [GameEndpoint("farc_hashes")]
    [MinimumRole(GameUserRole.Restricted)]
    //Stubbed to return a 410 Gone, so LBP3 doesn't spam us.
    //The game doesn't actually use this information for anything, so we don't allow server owners to replace this.
    public Response FarcHashes(RequestContext context) => Gone;
    
    //TODO: In the future this should allow you to have separate files per language since the game sends the language through the `language` query parameter.
    private static readonly Lazy<string?> DeveloperVideosFile
        = new(() =>
        {
            string path = Path.Combine(Environment.CurrentDirectory, "developer_videos.xml");
            
            return File.Exists(path) ? File.ReadAllText(path) : null;
        });
    
    [GameEndpoint("developer_videos")]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(OK)]
    public string? DeveloperVideos(RequestContext context)
    {
        bool created = DeveloperVideosFile.IsValueCreated;
        
        string? developerVideos = DeveloperVideosFile.Value;
        
        // Only log this warning once
        if(!created && developerVideos == null)
            context.Logger.LogWarning(BunkumCategory.Request, "developer_videos.xml file is missing! " +
                                                              "LBP will work without it, but it may be relevant to you if you are an advanced user.");
        
        return developerVideos; 
    }
    
    [GameEndpoint("gameState", ContentType.Plaintext, HttpMethods.Post)]
    [MinimumRole(GameUserRole.Restricted)]
    // It's unknown what an "invalid" result/state would be.
    // Since it sends information like the current create mode tool in use,
    // maybe it was used as a server-side anti-cheat to detect hacks/cheats?
    // The packet captures show `VALID` being returned, so we stub this method to that.
    //
    // Example request bodies:
    // {"currentLevel": ["pod", 0],"participants":  ["turecross321","","",""]}
    // {"currentLevel": ["user_local", 59],"inCreateMode": true,"participants":  ["turecross321","","",""],"selectedCreateTool": ""}
    // {"currentLevel": ["developer_adventure_planet", 349],"inStore": true,"participants":  ["turecross321","","",""]}
    // {"highlightedSearchResult": ["level",811],"currentLevel": ["pod", 0],"inStore": true,"participants":  ["turecross321","","",""]}
    public string GameState(RequestContext context) => "VALID";
    
    [GameEndpoint("ChallengeConfig.xml", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedLbp3ChallengeList ChallengeConfig(RequestContext context, IDateTimeProvider timeProvider)
    {
        //TODO: allow this to be controlled by the server owner, right now lets just send the game 0 challenges,
        //      so nothing appears in the challenges menu
        return new SerializedLbp3ChallengeList
        {
            TotalChallenges = 0,
            EndTime = (ulong)(timeProvider.Now.ToUnixTimeMilliseconds() * 1000),
            BronzeRankPercentage = 0,
            SilverRankPercentage = 0,
            GoldRankPercentage = 0,
            CycleTime = 0,
            Challenges = [],
        };
    }

    [GameEndpoint("tags")]
    [MinimumRole(GameUserRole.Restricted)]
    public string Tags(RequestContext context) => TagExtensions.AllTags;
}