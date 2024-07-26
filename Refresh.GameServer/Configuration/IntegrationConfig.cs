using Bunkum.Core.Configuration;

namespace Refresh.GameServer.Configuration;

/// <summary>
/// A configuration file representing options for various integrations like e-mail, discord, and others.
/// </summary>
public class IntegrationConfig : Config
{
    public override int CurrentConfigVersion => 4;
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
    public int DiscordWorkerFrequencySeconds { get; set; } = 60;
    public string DiscordNickname { get; set; } = "Refresh";
    public string DiscordAvatarUrl { get; set; } = "https://raw.githubusercontent.com/LittleBigRefresh/Branding/main/icons/refresh_512x.png";

    #endregion
    
    public string? GrafanaDashboardUrl { get; set; }
}