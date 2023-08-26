using System.Net;
using System.Net.Mail;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Configuration;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services;

public class SmtpService : EndpointService
{
    private readonly SmtpClient? _smtpClient;
    private readonly IntegrationConfig _integrationConfig;
    private readonly GameServerConfig _gameConfig;

    internal SmtpService(IntegrationConfig integrationConfig,
        GameServerConfig gameConfig,
        LoggerContainer<BunkumContext> logger) : base(logger)
    {
        this._integrationConfig = integrationConfig;
        this._gameConfig = gameConfig;

        if (!this._integrationConfig.SmtpEnabled) return;
        
        this._smtpClient = new SmtpClient(this._integrationConfig.SmtpHost)
        {
            Port = this._integrationConfig.SmtpPort,
            EnableSsl = this._integrationConfig.SmtpTlsEnabled,
            Credentials = new NetworkCredential(this._integrationConfig.SmtpUsername, this._integrationConfig.SmtpPassword),
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };
    }

    private bool SendEmail(string recipient, string subject, string body)
    {
        if (!this._integrationConfig.SmtpEnabled || this._smtpClient == null) return false;
        
        MailMessage message = new()
        {
            From = new MailAddress(this._integrationConfig.SmtpUsername),
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

    public bool SendEmailVerificationRequest(GameUser user, string code)
    {
        if (user.EmailAddress == null)
            throw new InvalidOperationException("Cannot send verification request for user with no email");
        
        return this.SendEmail(user.EmailAddress, $"E-mail Verification Code for {this._gameConfig.InstanceName}: {code}",
            $"""
            Hi {user.Username} (id: {user.UserId.ToString()}),
            
            We've received a request to verify your email address for {this._gameConfig.InstanceName}. Your verification code is: '{code}'.
            
            You can also verify your email if signed in via your browser by clicking the following link: {this._gameConfig.WebExternalUrl}/verify?code={code}
            
            If you didn't initiate this request, please disregard this message.
            
            Best regards,
                The {this._gameConfig.InstanceName} Team
            """);
    }
}