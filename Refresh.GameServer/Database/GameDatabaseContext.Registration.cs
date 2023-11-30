using System.Security.Cryptography;
using MongoDB.Bson;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial interface IGameDatabaseContext // Registration
{
    public GameUser CreateUser(string username, string emailAddress, bool skipChecks = false)
    {
        if (!skipChecks)
        {
            if (this.IsUsernameTaken(username))
                throw new InvalidOperationException("Cannot create a user with an existing username");
        
            if (this.IsEmailTaken(emailAddress))
                throw new InvalidOperationException("Cannot create a user with an existing email address");
        }

        emailAddress = emailAddress.ToLowerInvariant();
        
        GameUser user = new()
        {
            Username = username,
            EmailAddress = emailAddress,
            EmailAddressVerified = false,
            JoinDate = this.Time.Now,
        };

        this.Write(() =>
        {
            this.Add(user);
        });
        return user;
    }

    public GameUser CreateUserFromQueuedRegistration(QueuedRegistration registration, TokenPlatform? platform = null)
    {
        QueuedRegistration cloned = (QueuedRegistration)registration.Clone();

        this.Write(() =>
        {
            this.Remove(registration);
        });

        GameUser user = this.CreateUser(cloned.Username, cloned.EmailAddress);
        this.SetUserPassword(user, cloned.PasswordBcrypt);

        if (platform != null)
        {
            this.Write(() =>
            {
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (platform)
                {
                    case TokenPlatform.PS3:
                    case TokenPlatform.Vita:
                        user.PsnAuthenticationAllowed = true;
                        break;
                    case TokenPlatform.RPCS3:
                        user.RpcnAuthenticationAllowed = true;
                        break;
                }
            });
        }

        return user;
    }

    public bool IsUsernameTaken(string username)
    {
        return this.All<GameUser>().Any(u => u.Username == username) ||
               this.All<QueuedRegistration>().Any(r => r.Username == username);
    }
    
    public bool IsEmailTaken(string emailAddress)
    {
        return this.All<GameUser>().Any(u => u.EmailAddress == emailAddress) ||
               this.All<QueuedRegistration>().Any(r => r.EmailAddress == emailAddress);
    }

    public void AddRegistrationToQueue(string username, string emailAddress, string passwordBcrypt)
    {
        if (this.IsUsernameTaken(username))
            throw new InvalidOperationException("Cannot create a registration with an existing username");
        
        if (this.IsEmailTaken(emailAddress))
            throw new InvalidOperationException("Cannot create a user with an existing email address");
        
        QueuedRegistration registration = new()
        {
            Username = username,
            EmailAddress = emailAddress,
            PasswordBcrypt = passwordBcrypt,
            ExpiryDate = this.Time.Now + TimeSpan.FromDays(1), // This registration expires in 1 day
        };

        this.Write(() =>
        {
            this.Add(registration);
        });
    }

    public void RemoveRegistrationFromQueue(QueuedRegistration registration)
    {
        this.Write(() =>
        {
            this.Remove(registration);
        });
    }
    
    public void RemoveAllRegistrationsFromQueue()
    {
        this.Write(() =>
        {
            this.RemoveAll<QueuedRegistration>();
        });
    }
    
    public bool IsRegistrationExpired(QueuedRegistration registration) => registration.ExpiryDate < this.Time.Now;

    public QueuedRegistration? GetQueuedRegistrationByUsername(string username) 
        => this.All<QueuedRegistration>().FirstOrDefault(q => q.Username == username);
    
    public QueuedRegistration? GetQueuedRegistrationByObjectId(ObjectId id) 
        => this.All<QueuedRegistration>().FirstOrDefault(q => q.RegistrationId == id);
    

    public DatabaseList<QueuedRegistration> GetAllQueuedRegistrations()
        => new(this.All<QueuedRegistration>());
    
    public DatabaseList<EmailVerificationCode> GetAllVerificationCodes()
        => new(this.All<EmailVerificationCode>());
    
    public void VerifyUserEmail(GameUser user)
    {
        this.Write(() =>
        {
            user.EmailAddressVerified = true;
            this.RemoveRange(this.All<EmailVerificationCode>()
                .Where(c => c.User == user));
        });
    }

    public bool VerificationCodeMatches(GameUser user, string code) => 
        this.All<EmailVerificationCode>().Any(c => c.User == user && c.Code == code);
    
    public bool IsVerificationCodeExpired(EmailVerificationCode code) => code.ExpiryDate < this.Time.Now;

    private static string GenerateDigitCode()
    {
        ReadOnlySpan<byte> validChars = "0123456789"u8;
        Span<char> result = stackalloc char[6];
        Span<byte> randomBytes = stackalloc byte[6];

        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
            
        for (int i = 0; i < randomBytes.Length; i++)
        {
            int index = randomBytes[i] % validChars.Length;
            result[i] = (char)validChars[index];
        }

        return new string(result);
    }

    public EmailVerificationCode CreateEmailVerificationCode(GameUser user)
    {
        EmailVerificationCode verificationCode = new()
        {
            User = user,
            Code = GenerateDigitCode(),
            ExpiryDate = this.Time.Now + TimeSpan.FromDays(1),
        };

        this.Write(() =>
        {
            this.Add(verificationCode);
        });

        return verificationCode;
    }

    public void RemoveEmailVerificationCode(EmailVerificationCode code)
    {
        this.Write(() =>
        {
            this.Remove(code);
        });
    }
}