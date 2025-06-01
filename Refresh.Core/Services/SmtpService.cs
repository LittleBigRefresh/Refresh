using Bunkum.Core;
using Bunkum.Core.Services;
using DnsClient;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NotEnoughLogs;
using Refresh.GameServer.Configuration;
using Refresh.Database.Models.Users;

namespace Refresh.GameServer.Services;

public class SmtpService : EndpointService
{
    private readonly IntegrationConfig _integrationConfig;
    private readonly GameServerConfig _gameConfig;

    internal SmtpService(IntegrationConfig integrationConfig,
        GameServerConfig gameConfig,
        Logger logger) : base(logger)
    {
        this._integrationConfig = integrationConfig;
        this._gameConfig = gameConfig;
    }

    private bool SendEmail(string recipient, string subject, string body)
    {
        if (!this._integrationConfig.SmtpEnabled) return false;

        using SmtpClient client = new();

        SecureSocketOptions tlsOptions = SecureSocketOptions.None;

        if (this._integrationConfig is { SmtpTlsEnabled: true, SmtpPort: 587 })
            tlsOptions = SecureSocketOptions.StartTls;
        else if (this._integrationConfig.SmtpTlsEnabled)
            tlsOptions = SecureSocketOptions.SslOnConnect;
        
        client.Connect(this._integrationConfig.SmtpHost, this._integrationConfig.SmtpPort, tlsOptions);
        client.Authenticate(this._integrationConfig.SmtpUsername, this._integrationConfig.SmtpPassword);

        MimeMessage message = new();
        message.From.Add(new MailboxAddress(this._gameConfig.InstanceName, this._integrationConfig.SmtpUsername));
        message.To.Add(new MailboxAddress(recipient, recipient));

        message.Subject = subject;
        message.Body = new TextPart("plain") {
            Text = body,
        };

        try
        {
            client.Send(message);
        }
        catch (Exception e)
        {
            this.Logger.LogWarning(BunkumCategory.Service, $"Failed to send '{subject}' to '{recipient}':\n{e}");
            return false;
        }
        
        this.Logger.LogDebug(BunkumCategory.Service, $"Successfully sent '{subject}' to '{recipient}'");
        
        client.Disconnect(true);
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

    public bool SendPasswordResetRequest(GameUser user, string token)
    {
        if (user.EmailAddress == null)
            throw new InvalidOperationException("Cannot send verification request for user with no email");

        return this.SendEmail(user.EmailAddress, $"Password Reset Request for {this._gameConfig.InstanceName}",
            $"""
             Hi {user.Username} (id: {user.UserId.ToString()}),

             We've received a request to reset your password on your account for {this._gameConfig.InstanceName}.

             To reset your password, continue via your browser by clicking the following link: {this._gameConfig.WebExternalUrl}/resetPassword?token={token}

             If you didn't initiate this request, please disregard this message.

             Best regards,
                 The {this._gameConfig.InstanceName} Team
             """);
    }

    public bool CheckEmailDomainValidity(string emailAddress)
    {
        if (this._integrationConfig is not { SmtpVerifyDomain: true, SmtpEnabled: true })
            return true;
        
        string domain = emailAddress[(emailAddress.IndexOf('@') + 1)..];
        this.Logger.LogDebug(BunkumCategory.Authentication, $"Checking validity of email {emailAddress} (domain: {domain})");
        
        LookupClient dns = new();
        IDnsQueryResponse? records = dns.Query(domain, QueryType.MX);
        if (records == null) return false;

        return records.Answers.MxRecords().Any();
    }
}