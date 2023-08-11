using System.Net;
using System.Net.Mail;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services;

public class SmtpService : EndpointService
{
    private readonly SmtpClient? _smtpClient;
    private readonly IntegrationConfig _config;
    private readonly GameServerConfig _gameConfig;

    internal SmtpService(IntegrationConfig config, GameServerConfig gameConfig, LoggerContainer<BunkumContext> logger) : base(logger)
    {
        this._config = config;
        this._gameConfig = gameConfig;

        if (!this._config.SmtpEnabled) return;
        
        this._smtpClient = new SmtpClient(this._config.SmtpHost)
        {
            Port = this._config.SmtpPort,
            EnableSsl = this._config.SmtpTlsEnabled,
            Credentials = new NetworkCredential(this._config.SmtpUsername, this._config.SmtpPassword),
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };
    }

    private bool SendEmail(string recipient, string subject, string body)
    {
        if (!this._config.SmtpEnabled || this._smtpClient == null) return false;
        
        MailMessage message = new()
        {
            From = new MailAddress(this._config.SmtpUsername),
            To = { new MailAddress(recipient) },
            Subject = subject,
            Body = body,
        };

        try
        {
            this._smtpClient.Send(message);
        }
        catch (Exception e)
        {
            this.Logger.LogWarning(BunkumContext.Service, $"Failed to send '{subject}' to '{recipient}':\n{e}");
            return false;
        }
        
        this.Logger.LogDebug(BunkumContext.Service, $"Successfully sent '{subject}' to '{recipient}'");
        return true;
    }

    public bool EmailVerificationRequest(GameUser user, string recipient, string code)
    {
        return this.SendEmail(recipient, $"E-mail Verification Code for {this._gameConfig.InstanceName}: {code}",
            $"""
            Hi {user.Username} (id: {user.UserId.ToString()}),
            
            We've received a request to verify your email address for {this._gameConfig.InstanceName}. Your verification code is: '{code}'.
            
            If you didn't initiate this request, please disregard this message.
            
            Best regards,
                The {this._gameConfig.InstanceName} Team
            """);
    }
}