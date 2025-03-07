using Bunkum.Core.Configuration;

namespace Refresh.GameServer.Configuration;

/// <summary>
/// A configuration file representing options for various integrations like e-mail, discord, and others.
/// </summary>
public class IntegrationConfig : Config
{
    public override int CurrentConfigVersion => 7;
    public override int Version { get; set; }
    protected override void Migrate(int oldVer, dynamic oldConfig)
    {
        
    }

    #region SMTP
    // Settings for SMTP (E-mail sending)

    public bool SmtpEnabled { get; set; }
    public string SmtpHost { get; set; } = "mail.example.com";
    public ushort SmtpPort { get; set; } = 587;
    public bool SmtpTlsEnabled { get; set; } = true;
    public string SmtpUsername { get; set; } = "username@example.com";
    public string SmtpPassword { get; set; } = "P4$$w0rd";
    public bool SmtpVerifyDomain { get; set; } = true;

    #endregion

    #region Discord

    public bool DiscordWebhookEnabled { get; set; }
    public string DiscordWebhookUrl { get; set; } = "https://discord.com/api/webhooks/id/key";

    public bool DiscordStaffWebhookEnabled { get; set; }
    public string DiscordStaffWebhookUrl { get; set; } = "https://discord.com/api/webhooks/id/key";
    public int DiscordWorkerFrequencySeconds { get; set; } = 60;
    public string DiscordNickname { get; set; } = "Refresh";
    public string DiscordAvatarUrl { get; set; } = "https://raw.githubusercontent.com/LittleBigRefresh/Branding/main/icons/refresh_512x.png";

    #endregion
    
    #region AIPI
    
    public bool AipiEnabled { get; set; } = false;
    public string AipiBaseUrl { get; set; } = "http://localhost:5000";
    
    /// <summary>
    /// The threshold at which tags are discarded during EVA2 prediction.
    /// </summary>
    public float AipiThreshold { get; set; } = 0.85f;
    
    // in DO we store this statically, but this exposing this as a config option allows us to obscure which tags
    // are being blocked, because refresh is FOSS and DT could probably just look at it.
    public string[] AipiBannedTags { get; set; } = [];

    public bool AipiRestrictAccountOnDetection { get; set; } = false;
    
    #endregion

    #region Presence

    public bool PresenceEnabled { get; set; } = false;

    public string PresenceBaseUrl { get; set; } = "http://localhost:10073";
    
    public string PresenceSharedSecret { get; set; } = "SHARED_SECRET";

    #endregion
    
    public string? GrafanaDashboardUrl { get; set; }

    public string WebsiteLogoUrl { get; set; } = "https://github.com/LittleBigRefresh/Branding/blob/main/icons/refresh_transparent.svg";
}