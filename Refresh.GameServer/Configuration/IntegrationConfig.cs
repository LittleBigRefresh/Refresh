using Bunkum.HttpServer.Configuration;

namespace Refresh.GameServer.Configuration;

/// <summary>
/// A configuration file representing options for various integrations like e-mail, discord, and others.
/// </summary>
public class IntegrationConfig : Config
{
    public override int CurrentConfigVersion => 2;
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

    #endregion
    
    public string? GrafanaDashboardUrl { get; set; }
}